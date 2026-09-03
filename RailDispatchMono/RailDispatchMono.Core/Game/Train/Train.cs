using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    public Guid Id { get; }
    public TrainComposition Composition { get; }
    public Vector2 Position { get; private set; }
    public float DistanceAlongTrack { get; set; }
    public float TotalDistance { get; private set; }
    public TrackConnections Direction { get; private set; }
    public bool CanMove => Composition.CanMove;
    public float Length => Composition.Length;
    private BlockController? _blockController;

    private float _speed;
    private Vector2[] _vehicleOffsets = Array.Empty<Vector2>();

    public float Speed
    {
        get => _speed;
        set
        {
            float maxSpeed = float.MaxValue;
            foreach (var vehicle in Composition.Vehicles)
            {
                if (vehicle.Parameters.MaxSpeed < maxSpeed)
                    maxSpeed = vehicle.Parameters.MaxSpeed;
            }
            _speed = Math.Clamp(value, 0f, maxSpeed);
        }
    }

    public void SetBlockController(BlockController controller)
    {
        _blockController = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    public bool IsOnCurve => _isOnCurve;
    public Vector2 ArcCenter => _arcCenter;
    public float StartAngle => _arcStartAngle;
    public float SweepAngle => _arcSweepAngle;
    public float CurveProgressDistance => _curveDistance;
    public float CurveLength => _curveLength;
    public bool IsReversed => _isReversed;

#pragma warning disable CS0067
    public event EventHandler<TrainEventArgs>? VehicleAdded;
    public event EventHandler<TrainEventArgs>? VehicleRemoved;
    public event EventHandler<TrainEventArgs>? TrainCoupled;
    public event EventHandler<TrainEventArgs>? TrainDecoupled;
#pragma warning restore CS0067

    public class TrainEventArgs : EventArgs
    {
        public Train Train { get; }
        public Vehicle? Vehicle { get; }
        public int VehicleIndex { get; }

        public TrainEventArgs(Train train, Vehicle? vehicle, int index)
        {
            Train = train;
            Vehicle = vehicle;
            VehicleIndex = index;
        }
    }

    public Train(Vector2 spawnPosition, TrackConnections initialDirection, float speed, IEnumerable<Vehicle> vehicles)
    {
        Id = Guid.NewGuid();
        Position = spawnPosition;
        Direction = initialDirection;
        _speed = speed;
        _isReversed = false;

        Composition = new TrainComposition();
        foreach (var vehicle in vehicles)
            Composition.AddVehicle(vehicle);

        RebuildVehicleOffsets();

        _maxSpeed = float.MaxValue;
        foreach (var vehicle in Composition.Vehicles)
        {
            if (vehicle.Parameters.MaxSpeed < _maxSpeed)
                _maxSpeed = vehicle.Parameters.MaxSpeed;
        }
        _lastSignalSpeed = _maxSpeed;
        _targetSpeed = _maxSpeed;

        DistanceAlongTrack = 0f;
        TotalDistance = 0f;
        ResetCurveState();
        ResetTrajectory();
    }

    public Train(Vector2 spawnPosition, TrackConnections initialDirection, float speed)
        : this(spawnPosition, initialDirection, speed, Array.Empty<Vehicle>())
    {
    }

    public void SetMap(GameMap map) => _map = map ?? throw new ArgumentNullException(nameof(map));
    public Vector2 GetHeadPosition() => Position;

    public void SetPosition(Vector2 position)
    {
        Position = position;
        DistanceAlongTrack = 0f;
        TotalDistance = 0f;
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        ResetTrajectory();
    }

    public void SetDirection(TrackConnections direction)
    {
        ValidateDirection(direction);
        Direction = direction;
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        ResetTrajectory();
    }

    public void ResetSignalState()
    {
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
    }

    public bool IsOnTrack()
    {
        if (_map is null) return false;
        var cell = GetCurrentCell();
        return _map.TryGetTrack(cell, out var track) && track != null;
    }

    public float GetDistanceToVehicle(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));
        return GetMovementDistanceToVehicle(vehicleIndex);
    }

    public float GetTotalTrainLength() => Length;

    public Vector2 GetLastVehiclePosition()
    {
        if (Composition.Vehicles.Count == 0) return Position;
        return GetVehicleTransform(Composition.Vehicles.Count - 1).Position;
    }

    public TrackConnections GetLastVehicleDirection()
    {
        if (Composition.Vehicles.Count == 0) return Direction;
        int lastIndex = Composition.Vehicles.Count - 1;
        return VectorToDirection(GetVehicleTransform(lastIndex).Rotation);
    }

    public (Vector2 Position, float Rotation) GetVehicleTransform(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));

        if (_vehicleOffsets.Length != Composition.Vehicles.Count)
            RebuildVehicleOffsets();

        float distanceBehind = GetMovementDistanceToVehicle(vehicleIndex);

        // The head uses the exact current tangent. Other vehicles use the same
        // historical trajectory sample for both position and tangent, so they
        // enter and leave curves one after another instead of rotating together.
        if (distanceBehind <= MovementEpsilon)
            return (Position, GetRotation());

        if (TryGetTrajectoryTransformBehindHead(distanceBehind, out Vector2 trajectoryPosition, out float trajectoryRotation))
            return (trajectoryPosition, trajectoryRotation);

        // Before enough trajectory history exists, the consist is rigid on a
        // straight section. This also keeps F7 position-invariant.
        return (Position + _vehicleOffsets[vehicleIndex], GetDirectionAngle(Direction));
    }

    public IReadOnlyList<(Vector2 Position, float Distance)> GetTrajectoryHistory()
    {
        var result = new List<(Vector2 Position, float Distance)>(_trajectory.Count);
        foreach (var point in _trajectory)
            result.Add((point.Position, point.Distance));
        return result;
    }

    public int TrajectoryPointCount => _trajectory.Count;

    public (Vector2 Position, float Distance)? GetLastTrajectoryPoint()
    {
        if (_trajectory.Count == 0) return null;
        var last = _trajectory[^1];
        return (last.Position, last.Distance);
    }

    public float GetRotation()
    {
        Vector2 tangent;
        if (_isOnCurve && _curveLength > MovementEpsilon)
        {
            float progress = MathHelper.Clamp(_curveDistance / _curveLength, 0.0f, 1.0f);
            float angle = _arcStartAngle + (_arcSweepAngle * progress);
            tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
            if (_arcSweepAngle < 0.0f) tangent = -tangent;
        }
        else
        {
            tangent = DirectionToVector(Direction);
        }
        return MathF.Atan2(tangent.Y, tangent.X);
    }

    public List<Vector2> GetVehiclePositions(float vehicleSpacing = 1.0f)
    {
        var result = new List<Vector2>(Composition.Vehicles.Count);
        for (int i = 0; i < Composition.Vehicles.Count; i++)
            result.Add(GetVehicleTransform(i).Position);
        return result;
    }

    public float GetVehicleDistance(int vehicleIndex) => GetDistanceToVehicle(vehicleIndex);

    internal void RebuildVehicleOffsets()
    {
        _vehicleOffsets = new Vector2[Composition.Vehicles.Count];
        Vector2 directionVector = DirectionToVector(Direction);
        float distance = 0f;

        for (int i = 0; i < Composition.Vehicles.Count; i++)
        {
            _vehicleOffsets[i] = -directionVector * distance;
            distance += Composition.Vehicles[i].Parameters.Length;
        }
    }

    private SignalController? _signalController;
    private float _targetSpeed;
    private float _maxSpeed = 160f / 3.6f;
    private Signal? _lastSignal;
    private float _lastSignalSpeed;

    public void SetSignalController(SignalController controller)
    {
        _signalController = controller;
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
    }

    public Signal? GetNextSignal()
    {
        if (TryFindNextSignal(out var signal, out _))
            return signal;
        return null;
    }

    private bool TryFindNextSignal(out Signal? signal, out float distance)
    {
        signal = null;
        distance = 0f;
        if (_signalController == null || _map == null)
            return false;

        var current = GetCurrentCell();
        var direction = Direction;
        var visited = new HashSet<MapPosition>();
        float travelled = GetDistanceToBoundary();
        const int maxSteps = 2000;

        for (int step = 0; step < maxSteps; step++)
        {
            if (!visited.Add(current))
                return false;

            var signals = _signalController.GetSignalsAt(current);
            Signal? candidate = null;
            if (signals != null)
                candidate = signals.FirstOrDefault(s => s.Direction == direction);

            if (candidate != null)
            {
                signal = candidate;
                distance = SimulationScale.GridToMeters(MathF.Max(0f, travelled));
                return true;
            }

            if (!_map.TryGetTrack(current, out var track) || track == null)
                return false;

            var next = GetNextCell(current, direction);
            if (next.X < 0 || next.X >= _map.Size.Width || next.Y < 0 || next.Y >= _map.Size.Height)
                return false;
            if (!_map.TryGetTrack(next, out var nextTrack) || nextTrack == null)
                return false;

            var available = nextTrack.GetAvailableDirections();
            var opposite = GetOppositeDirection(direction);
            var nextDirection = available.FirstOrDefault(d => d != opposite);
            if (nextDirection == TrackConnections.None)
                return false;

            travelled += 1f;
            current = next;
            direction = nextDirection;
        }

        return false;
    }

    private float GetSignalDistance(Signal signal)
    {
        if (TryFindNextSignal(out var nextSignal, out var distance) && nextSignal == signal)
            return distance;
        return SimulationScale.GridToMeters(MathF.Max(0f, GetDistanceToBoundary()));
    }

    private float GetBrakingRate()
    {
        var locomotive = Composition.Locomotive;
        if (locomotive == null)
            return 20f;
        float baseBraking = locomotive.Parameters.Braking;
        if (baseBraking <= 0f)
            return 20f;
        float massFactor = GetMassPerformanceFactor();
        return MathF.Max(0.01f, baseBraking * massFactor);
    }

    private float GetSpeedFromSignal(Signal? signal)
    {
        if (signal == null) return _maxSpeed;
        float signalSpeed = signal.Aspect switch
        {
            SignalAspect.Stop => 0f,
            SignalAspect.StopStation => 0f,
            SignalAspect.Clear => _maxSpeed,
            SignalAspect.Warning => _maxSpeed * 0.5f,
            SignalAspect.Speed100 => 100f / 3.6f,
            SignalAspect.Speed40 => 40f / 3.6f,
            SignalAspect.Reserve1 => 120f / 3.6f,
            SignalAspect.Reserve2 => 80f / 3.6f,
            SignalAspect.Reserve3 => 60f / 3.6f,
            SignalAspect.Reserve4 => 30f / 3.6f,
            _ => _maxSpeed
        };
        float distance = GetSignalDistance(signal);
        float brakingRate = GetBrakingRate();
        if (signal.Aspect == SignalAspect.Stop || signal.Aspect == SignalAspect.StopStation)
        {
            const float reactionTime = 0.15f;
            const float stopOffsetCells = 0.8f;
            float stopOffsetMeters = SimulationScale.GridToMeters(stopOffsetCells);
            float frontHalfLengthCells = Composition.Vehicles.Count > 0
                ? Composition.Vehicles[0].Parameters.Length * 0.5f
                : 0f;
            float frontHalfLengthMeters = SimulationScale.GridToMeters(frontHalfLengthCells);
            float availableDistance = MathF.Max(
                0f,
                distance - stopOffsetMeters - frontHalfLengthMeters - Speed * reactionTime);
            return MathF.Min(_maxSpeed,
                MathF.Sqrt(MathF.Max(0f, 2f * brakingRate * availableDistance)));
        }
        if (Speed > signalSpeed && brakingRate > 0f)
        {
            float requiredBrakingDistance = MathF.Max(0f, (Speed * Speed - signalSpeed * signalSpeed) / (2f * brakingRate));
            if (distance > requiredBrakingDistance)
                return Speed;
        }
        return MathF.Min(signalSpeed, _maxSpeed);
    }

    public MapPosition GetCurrentCell()
    {
        return new MapPosition((int)MathF.Floor(Position.X), (int)MathF.Floor(Position.Y));
    }

    public float GetDistanceToBoundary()
    {
        MapPosition cell = GetCurrentCell();
        float result = Direction switch
        {
            TrackConnections.East => (cell.X + 1.0f) - Position.X,
            TrackConnections.West => Position.X - cell.X,
            TrackConnections.South => (cell.Y + 1.0f) - Position.Y,
            TrackConnections.North => Position.Y - cell.Y,
            _ => 0.0f
        };
        if (result < 0.0f) result = 0.0f;
        if (result < MovementEpsilon && result > 0) result = MovementEpsilon;
        return result;
    }
}