using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    // ============================================================
    // PUBLIC API
    // ============================================================

    public Guid Id { get; }
    public TrainComposition Composition { get; }
    public Vector2 Position { get; private set; }
    public float DistanceAlongTrack { get; set; }
    public float TotalDistance { get; private set; }
    public TrackConnections Direction { get; private set; }
    public bool CanMove => Composition.CanMove;
    public float Length => Composition.Length;

    private float _speed;
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

    public bool IsOnCurve => _isOnCurve;
    public Vector2 ArcCenter => _arcCenter;
    public float StartAngle => _arcStartAngle;
    public float SweepAngle => _arcSweepAngle;
    public float CurveProgressDistance => _curveDistance;
    public float CurveLength => _curveLength;

    // ============================================================
    // EVENTS
    // ============================================================

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

    // ============================================================
    // CONSTRUCTORS
    // ============================================================

    public Train(
        Vector2 spawnPosition,
        TrackConnections initialDirection,
        float speed,
        IEnumerable<Vehicle> vehicles)
    {
        Id = Guid.NewGuid();
        Position = spawnPosition;
        Direction = initialDirection;
        _speed = speed;

        Composition = new TrainComposition();
        foreach (var vehicle in vehicles)
        {
            Composition.AddVehicle(vehicle);
        }

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

    public Train(
        Vector2 spawnPosition,
        TrackConnections initialDirection,
        float speed)
        : this(spawnPosition, initialDirection, speed, Array.Empty<Vehicle>())
    {
    }

    // ============================================================
    // MAP
    // ============================================================

    public void SetMap(GameMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    // ============================================================
    // PUBLIC POSITION API
    // ============================================================

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

    public bool IsOnTrack()
    {
        if (_map is null) return false;
        var cell = GetCurrentCell();
        return _map.TryGetTrack(cell, out var track) && track != null;
    }

    // ============================================================
    // VEHICLE MANAGEMENT
    // ============================================================

    public float GetDistanceToVehicle(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));

        float distance = 0f;
        for (int i = 0; i < vehicleIndex; i++)
        {
            distance += Composition.Vehicles[i].Parameters.Length;
        }
        distance += Composition.Vehicles[vehicleIndex].Parameters.Length * 0.5f;
        return distance;
    }

    public float GetTotalTrainLength() => Length;

    public Vector2 GetLastVehiclePosition()
    {
        if (Composition.Vehicles.Count == 0) return Position;
        int lastIndex = Composition.Vehicles.Count - 1;
        float distanceToLast = GetDistanceToVehicle(lastIndex);
        return GetPositionBehindHead(distanceToLast);
    }

    public TrackConnections GetLastVehicleDirection()
    {
        if (Composition.Vehicles.Count == 0) return Direction;
        int lastIndex = Composition.Vehicles.Count - 1;
        var transform = GetVehicleTransform(lastIndex);
        return VectorToDirection(transform.Rotation);
    }

    // ============================================================
    // VEHICLE TRANSFORM
    // ============================================================

    public (Vector2 Position, float Rotation) GetVehicleTransform(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));

        float distanceBehindHead = GetDistanceToVehicle(vehicleIndex);
        float targetDistance = _totalTravelDistance - distanceBehindHead;

        if (_trajectory.Count == 0)
            return (Position, GetDirectionAngle(Direction));

        if (targetDistance <= _trajectory[0].Distance)
        {
            var first = _trajectory[0];
            var angle = _trajectory.Count > 1
                ? MathF.Atan2(_trajectory[1].Position.Y - first.Position.Y,
                               _trajectory[1].Position.X - first.Position.X)
                : GetDirectionAngle(Direction);
            return (first.Position, angle);
        }

        if (targetDistance >= _trajectory[_trajectory.Count - 1].Distance)
        {
            var last = _trajectory[_trajectory.Count - 1];
            var angle = _trajectory.Count > 1
                ? MathF.Atan2(last.Position.Y - _trajectory[_trajectory.Count - 2].Position.Y,
                               last.Position.X - _trajectory[_trajectory.Count - 2].Position.X)
                : GetDirectionAngle(Direction);
            return (last.Position, angle);
        }

        for (int i = _trajectory.Count - 1; i > 0; i--)
        {
            var curr = _trajectory[i];
            var prev = _trajectory[i - 1];

            if (targetDistance >= prev.Distance && targetDistance <= curr.Distance)
            {
                float segmentLength = curr.Distance - prev.Distance;
                float t = segmentLength > MovementEpsilon
                    ? (targetDistance - prev.Distance) / segmentLength
                    : 0f;

                Vector2 pos = Vector2.Lerp(prev.Position, curr.Position, t);
                Vector2 dir = curr.Position - prev.Position;
                float angle = dir != Vector2.Zero
                    ? MathF.Atan2(dir.Y, dir.X)
                    : GetDirectionAngle(Direction);

                return (pos, angle);
            }
        }

        return (Position, GetDirectionAngle(Direction));
    }

    // ============================================================
    // DEBUGGER HELPERS
    // ============================================================

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
        var last = _trajectory[_trajectory.Count - 1];
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
        if (Composition.Vehicles.Count == 0) return result;

        float distanceBehind = 0.0f;
        for (int i = 0; i < Composition.Vehicles.Count; i++)
        {
            var vehicle = Composition.Vehicles[i];
            if (i == 0)
            {
                result.Add(Position);
                distanceBehind = vehicle.Parameters.Length;
            }
            else
            {
                float spacing = vehicleSpacing > MovementEpsilon
                    ? vehicleSpacing
                    : vehicle.Parameters.Length;
                result.Add(GetPositionBehindHead(distanceBehind));
                distanceBehind += spacing;
            }
        }
        return result;
    }

    public float GetVehicleDistance(int vehicleIndex) => GetDistanceToVehicle(vehicleIndex);

    // ============================================================
    // SIGNAL CONTROL
    // ============================================================

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
        if (_signalController == null) return null;

        var currentCell = GetCurrentCell();
        var nextCell = GetNextCell(currentCell, Direction);

        System.Diagnostics.Debug.WriteLine($"[TRAIN] GetNextSignal - Current: {currentCell}, Next: {nextCell}, Dir: {Direction}");

        var currentSignals = _signalController.GetSignalsAt(currentCell);
        var signal = currentSignals?.FirstOrDefault(s => s.Direction == Direction);

        if (signal != null)
        {
            System.Diagnostics.Debug.WriteLine($"[TRAIN] ✅ Found signal at CURRENT cell: {signal.Aspect} ({signal.GetAspectName()})");
            return signal;
        }

        var nextSignals = _signalController.GetSignalsAt(nextCell);
        signal = nextSignals?.FirstOrDefault(s => s.Direction == Direction);

        if (signal != null)
        {
            System.Diagnostics.Debug.WriteLine($"[TRAIN] ✅ Found signal at NEXT cell: {signal.Aspect} ({signal.GetAspectName()})");
            return signal;
        }

        System.Diagnostics.Debug.WriteLine("[TRAIN] ❌ No signal found");
        return null;
    }

    private float GetSpeedFromSignal(Signal? signal)
    {
        if (signal == null) return _maxSpeed;

        return signal.Aspect switch
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
    }

    // ============================================================
    // PUBLIC GRID METHODS
    // ============================================================

    public MapPosition GetCurrentCell()
    {
        return new MapPosition(
            (int)MathF.Floor(Position.X),
            (int)MathF.Floor(Position.Y));
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

        System.Diagnostics.Debug.WriteLine(
            $"[BOUNDARY] Cell:{cell} Dir:{Direction} Pos:{Position} Result:{result:F6}");

        return result;
    }
}