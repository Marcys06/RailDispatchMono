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

public sealed class StationController
{
    private sealed class DwellState
    {
        public Station Station { get; }
        public float RemainingSeconds { get; set; }
        public DwellState(Station station) { Station = station; RemainingSeconds = station.DwellTimeSeconds; }
    }

    private readonly List<Station> _stations = new();
    private readonly Dictionary<Guid, DwellState> _dwellingTrains = new();
    private readonly Dictionary<Guid, float> _generationTimers = new();
    private readonly Dictionary<Guid, Guid> _completedStationVisits = new();
    private readonly PassengerManager _passengers;
    private readonly ITrainStopDecision _stopDecision;
    private readonly IPassengerService _passengerService;
    private readonly IPassengerDemandProvider _demandProvider;

    public IReadOnlyList<Station> Stations => _stations;
    public PassengerManager Passengers => _passengers;
    public ITrainStopDecision StopDecision => _stopDecision;
    public IPassengerService PassengerService => _passengerService;
    public IPassengerDemandProvider DemandProvider => _demandProvider;

    public StationController(PassengerManager? passengers = null, ITrainStopDecision? stopDecision = null, IPassengerService? passengerService = null, IPassengerDemandProvider? demandProvider = null)
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
        if (_stations.Any(s => s.GetCells().Any(station.Contains))) throw new InvalidOperationException("Station area overlaps an existing station.");
        _stations.Add(station);
        _generationTimers[station.Id] = station.PassengerGenerationIntervalSeconds;
    }

    public bool RemoveStation(Station station)
    {
        if (station == null || !_stations.Remove(station)) return false;
        _generationTimers.Remove(station.Id);
        foreach (var trainId in _completedStationVisits.Where(x => x.Value == station.Id).Select(x => x.Key).ToList()) _completedStationVisits.Remove(trainId);
        return true;
    }

    public void Clear()
    {
        _stations.Clear();
        _generationTimers.Clear();
        _dwellingTrains.Clear();
        _completedStationVisits.Clear();
    }

    public Station? GetStationAt(MapPosition position) => _stations.FirstOrDefault(s => s.Contains(position));
    public IReadOnlyList<Station> GetStationsAt(MapPosition position) => _stations.Where(s => s.Contains(position)).ToList();

    public void Update(float deltaTime)
    {
        deltaTime = MathF.Max(0f, deltaTime);
        foreach (var station in _stations)
        {
            if (!station.PassengerGenerationEnabled || station.PassengerGenerationIntervalSeconds <= 0f) continue;
            float timer = _generationTimers.TryGetValue(station.Id, out var value) ? value - deltaTime : station.PassengerGenerationIntervalSeconds - deltaTime;
            while (timer <= 0f) { GeneratePassengers(station); timer += station.PassengerGenerationIntervalSeconds; }
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
            _passengers.CreatePassenger(origin, destination); generated++;
        }
        if (generated > 0) DebugManager.Log($"[STATION] {origin.Name}: generated {generated} passenger(s)");
    }

    public bool BeforeTrainUpdate(TrainClass train, float deltaTime)
    {
        if (!_dwellingTrains.TryGetValue(train.Id, out var dwell)) return false;
        dwell.RemainingSeconds -= MathF.Max(0f, deltaTime); train.Speed = 0f;
        if (dwell.RemainingSeconds > 0f) return true;
        _dwellingTrains.Remove(train.Id); return false;
    }

    public void AfterTrainUpdate(TrainClass train, float deltaTime)
    {
        ClearCompletedVisitIfTrainLeftStation(train);
        var currentStation = FindStationAtTrain(train);
        if (currentStation != null && _stopDecision.ShouldStopAt(train, currentStation) && train.Speed <= 0.75f && (!_completedStationVisits.TryGetValue(train.Id, out var completedStationId) || completedStationId != currentStation.Id))
        {
            train.Speed = 0f;
            if (!_dwellingTrains.ContainsKey(train.Id))
            {
                _passengerService.ServiceTrainAtStation(train, currentStation);
                _dwellingTrains[train.Id] = new DwellState(currentStation);
                _completedStationVisits[train.Id] = currentStation.Id;
            }
            return;
        }
        var nextStation = FindNextStation(train);
        if (nextStation == null || !_stopDecision.ShouldStopAt(train, nextStation)) return;
        float distance = EstimateDistanceToStation(train, nextStation), braking = GetTrainBraking(train);
        if (braking <= 0f || distance <= 0f) return;
        float available = MathF.Max(0f, distance - nextStation.StopRadius);
        float safeSpeed = MathF.Sqrt(MathF.Max(0f, 2f * braking * available));
        if (safeSpeed < train.Speed) train.Speed = safeSpeed;
    }

    public bool IsDwelling(TrainClass train) => _dwellingTrains.ContainsKey(train.Id);

    private void ClearCompletedVisitIfTrainLeftStation(TrainClass train)
    {
        if (!_completedStationVisits.TryGetValue(train.Id, out var stationId)) return;
        var station = _stations.FirstOrDefault(s => s.Id == stationId);
        if (station == null || !IsTrainWithinStationArea(train, station)) _completedStationVisits.Remove(train.Id);
    }

    private static bool IsTrainWithinStationArea(TrainClass train, Station station)
    {
        var cell = train.GetCurrentCell();
        if (!station.Contains(cell)) return false;
        Vector2 stationCenter = new(station.Position.X + station.Width / 2f, station.Position.Y + station.Height / 2f);
        return Vector2.DistanceSquared(train.Position, stationCenter) <= MathF.Max(1f, station.StopRadius * station.StopRadius);
    }

    private Station? FindStationAtTrain(TrainClass train) => _stations.FirstOrDefault(s => s.Contains(train.GetCurrentCell()));
    private Station? FindNextStation(TrainClass train) => _stations.FirstOrDefault(s => s.Id != FindStationAtTrain(train)?.Id);
    private static float EstimateDistanceToStation(TrainClass train, Station station) => Vector2.Distance(train.Position, new Vector2(station.Position.X + station.Width / 2f, station.Position.Y + station.Height / 2f));
    private static float GetTrainBraking(TrainClass train) => MathF.Max(0.1f, train.MaxSpeed * 0.5f);
}