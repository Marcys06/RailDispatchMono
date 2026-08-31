using Microsoft.Xna.Framework;
using RailDispatchMono.Core;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Passengers;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

using TrainClass = RailDispatchMono.Core.Game.Train.Train;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// Coordinates station detection and train dwell, while delegating the actual
/// stop decision and passenger handling to independent services.
/// </summary>
public sealed class StationController
{
    private sealed class DwellState
    {
        public Station Station { get; }
        public float RemainingSeconds { get; set; }

        public DwellState(Station station)
        {
            Station = station;
            RemainingSeconds = station.DwellTimeSeconds;
        }
    }

    private readonly List<Station> _stations = new();
    private readonly Dictionary<Guid, DwellState> _dwellingTrains = new();
    private readonly Dictionary<Guid, float> _generationTimers = new();
    private readonly PassengerManager _passengers;
    private readonly ITrainStopDecision _stopDecision;
    private readonly IPassengerService _passengerService;
    private readonly IPassengerDemandProvider _demandProvider;

    public IReadOnlyList<Station> Stations => _stations;
    public PassengerManager Passengers => _passengers;
    public ITrainStopDecision StopDecision => _stopDecision;
    public IPassengerService PassengerService => _passengerService;
    public IPassengerDemandProvider DemandProvider => _demandProvider;

    public StationController(
        PassengerManager? passengers = null,
        ITrainStopDecision? stopDecision = null,
        IPassengerService? passengerService = null,
        IPassengerDemandProvider? demandProvider = null)
    {
        _passengers = passengers ?? new PassengerManager();
        _stopDecision = stopDecision ?? new DefaultTrainStopDecision();
        _passengerService = passengerService ?? new DefaultPassengerService(_passengers);
        _demandProvider = demandProvider ?? new RandomPassengerDemandProvider();
    }

    public void AddStation(Station station)
    {
        if (station == null) throw new ArgumentNullException(nameof(station));
        if (_stations.Any(s => s.Id == station.Id)) return;
        if (_stations.Any(s => s.GetCells().Any(station.Contains)))
            throw new InvalidOperationException("Station area overlaps an existing station.");

        _stations.Add(station);
        _generationTimers[station.Id] = station.PassengerGenerationIntervalSeconds;
    }

    public bool RemoveStation(Station station)
    {
        if (station == null || !_stations.Remove(station)) return false;
        _generationTimers.Remove(station.Id);
        return true;
    }

    public Station? GetStationAt(MapPosition position) =>
        _stations.FirstOrDefault(s => s.Contains(position));

    public IReadOnlyList<Station> GetStationsAt(MapPosition position) =>
        _stations.Where(s => s.Contains(position)).ToList();

    /// <summary>Advances automatic passenger generation.</summary>
    public void Update(float deltaTime)
    {
        deltaTime = MathF.Max(0f, deltaTime);
        foreach (var station in _stations)
        {
            if (!station.PassengerGenerationEnabled || station.PassengerGenerationIntervalSeconds <= 0f)
                continue;

            float timer = _generationTimers.TryGetValue(station.Id, out var value)
                ? value - deltaTime
                : station.PassengerGenerationIntervalSeconds - deltaTime;

            while (timer <= 0f)
            {
                GeneratePassengers(station);
                timer += station.PassengerGenerationIntervalSeconds;
            }

            _generationTimers[station.Id] = timer;
        }
    }

    private void GeneratePassengers(Station origin)
    {
        int waiting = _passengers.GetWaitingCount(origin);
        int available = Math.Max(0, origin.PassengerWaitingCapacity - waiting);
        int requested = Math.Min(Math.Max(0, origin.PassengerGenerationBatchSize), available);
        if (requested <= 0 || _stations.Count < 2) return;

        int generated = 0;
        foreach (var destination in _demandProvider.GetDestinations(origin, _stations, requested))
        {
            if (generated >= requested || destination.Id == origin.Id) break;
            _passengers.CreatePassenger(origin, destination);
            generated++;
        }

        if (generated > 0)
            DebugManager.Log($"[STATION] {origin.Name}: generated {generated} passenger(s)");
    }

    public bool BeforeTrainUpdate(TrainClass train, float deltaTime)
    {
        if (!_dwellingTrains.TryGetValue(train.Id, out var dwell)) return false;

        dwell.RemainingSeconds -= MathF.Max(0f, deltaTime);
        train.Speed = 0f;

        if (dwell.RemainingSeconds <= 0)
        {
            _dwellingTrains.Remove(train.Id);
            train.Speed = 0.1f; // <-- MA£Y IMPULS DO RUSZENIA
            DebugManager.Train($"[STATION] Train {train.Id} released with impulse");
            return false;
        }
        if (dwell.RemainingSeconds > 0f) return true;

        _dwellingTrains.Remove(train.Id);
        return false;
    }

    public void AfterTrainUpdate(TrainClass train, float deltaTime)
    {
        var currentStation = FindStationAtTrain(train);
        if (currentStation != null && _stopDecision.ShouldStopAt(train, currentStation) &&
            train.Speed <= 0.75f)
        {
            train.Speed = 0f;
            if (!_dwellingTrains.ContainsKey(train.Id))
            {
                var result = _passengerService.ServiceTrainAtStation(train, currentStation);
                _dwellingTrains[train.Id] = new DwellState(currentStation);
                DebugManager.Log($"[STATION] Train {train.Id} arrived at {currentStation.Name}; " +
                                 $"alighted={result.Alighted}, boarded={result.Boarded}, dwell started.");
            }
            return;
        }

        var nextStation = FindNextStation(train);
        if (nextStation == null || !_stopDecision.ShouldStopAt(train, nextStation)) return;

        float distance = EstimateDistanceToStation(train, nextStation);
        float braking = GetTrainBraking(train);
        if (braking <= 0f || distance <= 0f) return;

        float available = MathF.Max(0f, distance - nextStation.StopRadius);
        float safeSpeed = MathF.Sqrt(MathF.Max(0f, 2f * braking * available));
        if (safeSpeed < train.Speed)
        {
            train.Speed = safeSpeed;
            DebugManager.Log($"[STATION] Braking for {nextStation.Name}: distance={distance:F2}, safe={safeSpeed:F2} m/s");
        }
    }

    public bool IsDwelling(TrainClass train) => _dwellingTrains.ContainsKey(train.Id);

    private Station? FindStationAtTrain(TrainClass train)
    {
        var cell = train.GetCurrentCell();
        var station = _stations.FirstOrDefault(s => s.PassengerServiceEnabled && s.Contains(cell));
        if (station == null) return null;

        Vector2 stationCenter = new(
            station.Position.X + station.Width / 2f,
            station.Position.Y + station.Height / 2f);

        float distance = Vector2.Distance(train.Position, stationCenter);

        // Jeœli poci¹g jest bardzo blisko centrum i ma nisk¹ prêdkoœæ, to zatrzymaj
        // Jeœli poci¹g oddala siê od centrum, nie zatrzymuj
        if (distance <= MathF.Max(station.Width, station.Height) / 2f + station.StopRadius + 0.5f)
        {
            // SprawdŸ, czy poci¹g siê oddala
            if (_dwellingTrains.ContainsKey(train.Id) && train.Speed > 0.1f)
            {
                // W³aœnie opuœci³ postój - daj mu szansê odjechaæ
                return null;
            }
            return station;
        }
        return null;
    }

    private Station? FindNextStation(TrainClass train)
    {
        Station? best = null;
        float bestDistance = float.MaxValue;
        var current = train.GetCurrentCell();

        foreach (var station in _stations)
        {
            if (!station.PassengerServiceEnabled || station.Contains(current)) continue;
            if (!_stopDecision.ShouldStopAt(train, station) || !IsStationAhead(train, station, current)) continue;

            float distance = EstimateDistanceToStation(train, station);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = station;
            }
        }

        return best;
    }

    private static bool IsStationAhead(TrainClass train, Station station, MapPosition current)
    {
        foreach (var cell in station.GetCells())
        {
            int dx = cell.X - current.X;
            int dy = cell.Y - current.Y;

            bool ahead = train.Direction switch
            {
                TrackConnections.East => dx > 0 && Math.Abs(dy) <= 1,
                TrackConnections.West => dx < 0 && Math.Abs(dy) <= 1,
                TrackConnections.South => dy > 0 && Math.Abs(dx) <= 1,
                TrackConnections.North => dy < 0 && Math.Abs(dx) <= 1,
                _ => false
            };

            if (ahead) return true;
        }

        return false;
    }

    private static float EstimateDistanceToStation(TrainClass train, Station station)
    {
        float best = float.MaxValue;
        foreach (var cell in station.GetCells())
        {
            float dx = cell.X + 0.5f - train.Position.X;
            float dy = cell.Y + 0.5f - train.Position.Y;
            best = MathF.Min(best, MathF.Sqrt(dx * dx + dy * dy));
        }
        return best;
    }

    private static float GetTrainBraking(TrainClass train)
    {
        float braking = 0f;
        foreach (var vehicle in train.Composition.Vehicles)
            braking = MathF.Max(braking, vehicle.Parameters.Braking);
        return braking;
    }
}
