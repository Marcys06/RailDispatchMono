using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainManager
{
    private sealed class SignalSpeedState
    {
        public Signal? ApproachingSignal { get; set; }
        public float CurrentLimit { get; set; }
    }

    private readonly GameMap _map;
    private readonly List<Train> _trains = new();
    private readonly List<Train> _trainsToAdd = new();
    private readonly List<Train> _trainsToRemove = new();
    private readonly Dictionary<Guid, SignalSpeedState> _signalSpeedStates = new();

    public static TrainManager? Current { get; private set; }
    public GameMap Map => _map;
    public IReadOnlyList<Train> Trains => _trains;
    public StationController StationController { get; private set; }
    public TrainCollisionController CollisionController { get; }
    public BlockController? BlockController => _blockController;

    public TrainManager(GameMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        StationController = new StationController();
        CollisionController = new TrainCollisionController(_map, this);
        Current = this;
    }

    public void Add(Train train)
    {
        if (train == null) throw new ArgumentNullException(nameof(train));
        if (_trains.Contains(train) || _trainsToAdd.Contains(train)) return;
        if (IsSpawnBlocked(train))
        {
            DebugManager.LogWarning($"[COLLISION] Train {train.Id.ToString()[..8]} spawn rejected: occupied track cell.");
            return;
        }
        train.SetMap(_map);
        _trainsToAdd.Add(train);
        _signalSpeedStates[train.Id] = new SignalSpeedState
        {
            CurrentLimit = GetPhysicalMaxSpeed(train)
        };
        train.SetEffectiveSignalSpeed(GetPhysicalMaxSpeed(train));
    }

    public Train CreateTrain(Vector2 position, TrackConnections direction, float speed)
    {
        var train = new Train(position, direction, speed);
        train.SetMap(_map);
        Add(train);
        return train;
    }

    public Train CreateTrain(MapPosition cell, TrackConnections direction, float speed) =>
        CreateTrain(new Vector2(cell.X + 0.5f, cell.Y + 0.5f), direction, speed);

    public bool Remove(Train train)
    {
        if (train == null || !_trains.Contains(train)) return false;
        _trainsToRemove.Add(train);
        return true;
    }

    private BlockController? _blockController;

    public void ClearAll()
    {
        foreach (var train in _trains)
            if (!_trainsToRemove.Contains(train))
                _trainsToRemove.Add(train);
    }

    public void Initialize(BlockController blockController) => _blockController = blockController;

    [Obsolete("StationController is initialized automatically. Configure TrainManager.StationController instead.")]
    public void InitializeStations(StationController stationController)
    {
        if (stationController == null) throw new ArgumentNullException(nameof(stationController));
        StationController = stationController;
    }

    public void Update(float deltaTime)
    {
        foreach (var train in _trainsToAdd)
        {
            if (!_trains.Contains(train))
            {
                train.SetMap(_map);
                _trains.Add(train);
            }
        }
        _trainsToAdd.Clear();

        foreach (var train in _trainsToRemove)
        {
            _trains.Remove(train);
            _signalSpeedStates.Remove(train.Id);
        }
        _trainsToRemove.Clear();

        StationController.Update(deltaTime);

        foreach (var train in _trains)
        {
            bool holdAtStation = StationController.BeforeTrainUpdate(train, deltaTime);
            if (holdAtStation)
            {
                train.RadioStop();
                continue;
            }

            if (CollisionController.ShouldRadioStop(train))
            {
                train.RadioStop();
                DebugManager.Log($"[COLLISION] RadioStop: train {train.Id.ToString()[..8]} has another train within {2f:F0} cells without a protecting signal.");
                continue;
            }

            train.ClearRadioStop();
            UpdateSignalSpeedState(train);
            train.Update(deltaTime);
            StationController.AfterTrainUpdate(train, deltaTime);
        }

        _blockController?.Update(deltaTime);
    }

    private void UpdateSignalSpeedState(Train train)
    {
        if (!_signalSpeedStates.TryGetValue(train.Id, out var state))
        {
            state = new SignalSpeedState { CurrentLimit = GetPhysicalMaxSpeed(train) };
            _signalSpeedStates[train.Id] = state;
        }

        Signal? nextSignal = train.GetNextSignal();

        // A changed next-signal identity means that the previous signal was
        // passed. The identity is the Signal object/ID, not its grid cell, so
        // two directional signals sharing one cell remain independent.
        if (state.ApproachingSignal != null && state.ApproachingSignal != nextSignal)
        {
            state.CurrentLimit = GetSignalSpeedLimit(train, state.ApproachingSignal);
            state.ApproachingSignal.NotifyTrainPassed(train);
            DebugManager.Log($"[SIGNAL] Train {train.Id.ToString()[..8]} passed {state.ApproachingSignal.Name}; new limit={state.CurrentLimit * 3.6f:F1} km/h");
        }

        if (nextSignal != null)
        {
            state.ApproachingSignal = nextSignal;
        }
        else if (state.ApproachingSignal != null)
        {
            state.CurrentLimit = GetSignalSpeedLimit(train, state.ApproachingSignal);
            state.ApproachingSignal.NotifyTrainPassed(train);
            DebugManager.Log($"[SIGNAL] Train {train.Id.ToString()[..8]} passed {state.ApproachingSignal.Name}; new limit={state.CurrentLimit * 3.6f:F1} km/h");
            state.ApproachingSignal = null;
        }

        // This is the persistent limit from the last passed signal. Train's
        // existing braking code remains responsible for approaching the next
        // signal; this value prevents a future Clear from raising speed before
        // that Clear has actually been passed.
        train.SetEffectiveSignalSpeed(state.CurrentLimit);
    }

    private static float GetPhysicalMaxSpeed(Train train)
    {
        float maxSpeed = float.MaxValue;
        foreach (var vehicle in train.Composition.Vehicles)
            maxSpeed = MathF.Min(maxSpeed, vehicle.Parameters.MaxSpeed);
        return maxSpeed == float.MaxValue ? 0f : maxSpeed;
    }

    private static float GetSignalSpeedLimit(Train train, Signal signal)
    {
        return signal.Aspect switch
        {
            SignalAspect.Stop => 0f,
            SignalAspect.StopStation => 0f,
            SignalAspect.Clear => GetPhysicalMaxSpeed(train),
            _ => MathF.Min(GetPhysicalMaxSpeed(train), signal.GetSpeedLimitKmh() / 3.6f)
        };
    }

    public bool IsCellOccupied(MapPosition cell)
    {
        foreach (var train in _trains)
            if (train.GetCurrentCell() == cell)
                return true;
        return false;
    }

    public Train? GetTrainAtCell(MapPosition cell)
    {
        foreach (var train in _trains)
            if (train.GetCurrentCell() == cell)
                return train;
        return null;
    }

    public List<Train> GetTrainsInRadius(Vector2 center, float radius)
    {
        var result = new List<Train>();
        float radiusSquared = radius * radius;
        foreach (var train in _trains)
            if (Vector2.DistanceSquared(center, train.Position) <= radiusSquared)
                result.Add(train);
        return result;
    }

    public int Count => _trains.Count;
    public bool HasAnyTrains => _trains.Count > 0;

    public List<Vector2> GetAllHeadPositions()
    {
        var positions = new List<Vector2>(_trains.Count);
        foreach (var train in _trains)
            positions.Add(train.Position);
        return positions;
    }

    public Dictionary<Train, Vector2[]> GetAllVehiclePositions(float vehicleSpacing = 1.0f)
    {
        var result = new Dictionary<Train, Vector2[]>();
        foreach (var train in _trains)
            result[train] = train.GetVehiclePositions(vehicleSpacing).ToArray();
        return result;
    }

    public Dictionary<Train, float> GetAllRotations()
    {
        var result = new Dictionary<Train, float>();
        foreach (var train in _trains)
            result[train] = train.GetRotation();
        return result;
    }

    public Dictionary<Train, (Vector2 Position, float Rotation)[]> GetAllVehicleTransforms()
    {
        var result = new Dictionary<Train, (Vector2, float)[]>();
        foreach (var train in _trains)
        {
            var transforms = new (Vector2, float)[train.Composition.Vehicles.Count];
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
                transforms[i] = train.GetVehicleTransform(i);
            result[train] = transforms;
        }
        return result;
    }

    private bool IsSpawnBlocked(Train candidate)
    {
        var occupiedCells = new HashSet<MapPosition>();
        foreach (var train in _trains.Concat(_trainsToAdd))
        {
            if (train.Id == candidate.Id) continue;
            foreach (var position in train.GetVehiclePositions())
            {
                occupiedCells.Add(new MapPosition(
                    (int)MathF.Floor(position.X),
                    (int)MathF.Floor(position.Y)));
            }
        }

        foreach (var position in candidate.GetVehiclePositions())
        {
            var cell = new MapPosition(
                (int)MathF.Floor(position.X),
                (int)MathF.Floor(position.Y));
            if (occupiedCells.Contains(cell))
                return true;
        }
        return false;
    }
}
