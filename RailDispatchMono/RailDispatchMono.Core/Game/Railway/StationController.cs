using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Passengers;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// Station stop controller. It is deliberately outside Train so the train
/// physics remains reusable and station policy can evolve independently.
///
/// The controller currently uses grid-distance look-ahead. It is sufficient for
/// station stopping on the existing cell-based network and is designed to be
/// replaced by TrackRoute distance when route planning becomes authoritative.
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
    private readonly PassengerManager _passengers;

    public IReadOnlyList<Station> Stations => _stations;
    public PassengerManager Passengers => _passengers;

    public StationController(PassengerManager? passengers = null)
    {
        _passengers = passengers ?? new PassengerManager();
    }

    public void AddStation(Station station)
    {
        if (station == null) throw new ArgumentNullException(nameof(station));
        if (_stations.All(s => s.Id != station.Id))
            _stations.Add(station);
    }

    public bool RemoveStation(Station station)
    {
        if (station == null) return false;
        return _stations.Remove(station);
    }

    public Station? GetStationAt(MapPosition position) =>
        _stations.FirstOrDefault(s => s.Position == position);

    /// <summary>
    /// Returns true when the train should be held instead of running Train.Update.
    /// </summary>
    public bool BeforeTrainUpdate(Train train, float deltaTime)
    {
        if (!_dwellingTrains.TryGetValue(train.Id, out var dwell))
            return false;

        dwell.RemainingSeconds -= MathF.Max(0f, deltaTime);
        train.Speed = 0f;

        if (dwell.RemainingSeconds > 0f)
            return true;

        _dwellingTrains.Remove(train.Id);
        return false;
    }

    /// <summary>
    /// Called after normal movement. It applies station braking as a speed cap,
    /// then enters dwell once the train is physically close enough to the stop.
    /// </summary>
    public void AfterTrainUpdate(Train train, float deltaTime)
    {
        var currentStation = FindStationAtTrain(train);
        if (currentStation != null && currentStation.PassengerServiceEnabled && train.Speed <= 0.75f)
        {
            train.Speed = 0f;
            if (!_dwellingTrains.ContainsKey(train.Id))
            {
                ServiceStation(train, currentStation);
                _dwellingTrains[train.Id] = new DwellState(currentStation);
                DebugManager.Log($"[STATION] Train {train.Id} arrived at {currentStation.Name}; dwell started.");
            }
            return;
        }

        var nextStation = FindNextStation(train);
        if (nextStation == null)
            return;

        float distance = EstimateDistanceToStation(train, nextStation);
        float braking = GetTrainBraking(train);
        if (braking <= 0f || distance <= 0f)
            return;

        // Leave a small operational margin so the station controller does not
        // depend on an exact cell-center collision.
        float available = MathF.Max(0f, distance - nextStation.StopRadius);
        float safeSpeed = MathF.Sqrt(MathF.Max(0f, 2f * braking * available));
        if (safeSpeed < train.Speed)
        {
            train.Speed = safeSpeed;
            DebugManager.Log($"[STATION] Braking for {nextStation.Name}: distance={distance:F2}, safe={safeSpeed:F2} m/s");
        }
    }

    public bool IsDwelling(Train train) => _dwellingTrains.ContainsKey(train.Id);

    private void ServiceStation(Train train, Station station)
    {
        _passengers.AlightPassengers(train, station);
        _passengers.BoardPassengers(train, station);
    }

    private Station? FindStationAtTrain(Train train)
    {
        var cell = train.GetCurrentCell();
        return _stations.FirstOrDefault(s =>
            s.PassengerServiceEnabled &&
            s.Position == cell &&
            Vector2.Distance(train.Position, new Vector2(s.Position.X + 0.5f, s.Position.Y + 0.5f)) <= s.StopRadius + 0.5f);
    }

    private Station? FindNextStation(Train train)
    {
        Station? best = null;
        float bestDistance = float.MaxValue;
        var current = train.GetCurrentCell();

        foreach (var station in _stations)
        {
            if (!station.PassengerServiceEnabled || station.Position == current)
                continue;

            int dx = station.Position.X - current.X;
            int dy = station.Position.Y - current.Y;
            bool ahead = train.Direction switch
            {
                TrackConnections.East => dx > 0 && Math.Abs(dy) <= 1,
                TrackConnections.West => dx < 0 && Math.Abs(dy) <= 1,
                TrackConnections.South => dy > 0 && Math.Abs(dx) <= 1,
                TrackConnections.North => dy < 0 && Math.Abs(dx) <= 1,
                _ => false
            };

            if (!ahead)
                continue;

            float distance = EstimateDistanceToStation(train, station);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = station;
            }
        }

        return best;
    }

    private static float EstimateDistanceToStation(Train train, Station station)
    {
        var current = train.GetCurrentCell();
        float dx = station.Position.X - train.Position.X;
        float dy = station.Position.Y - train.Position.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    private static float GetTrainBraking(Train train)
    {
        float braking = 0f;
        foreach (var vehicle in train.Composition.Vehicles)
            braking = MathF.Max(braking, vehicle.Parameters.Braking);
        return braking;
    }
}
