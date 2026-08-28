using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    // ============================================================
    // PUBLIC API
    // ============================================================

    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public Vector2 Position { get; private set; }

    public float DistanceAlongTrack { get; set; }

    public float TotalDistance { get; private set; }

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
    private float _speed;

    public TrackConnections Direction { get; private set; }

    public bool CanMove => Composition.CanMove;

    public float Length => Composition.Length;

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
    // MAP
    // ============================================================

    private GameMap? _map;

    // ============================================================
    // CURVE MOTION STATE
    // ============================================================

    private bool _isOnCurve;
    private MapPosition _curveCell;
    private TrackConnections _curveEntrySide;
    private TrackConnections _curveExitSide;
    private Vector2 _arcCenter;
    private float _arcStartAngle;
    private float _arcSweepAngle;
    private float _curveDistance;
    private float _curveLength;

    // ============================================================
    // CONSTANTS
    // ============================================================

    private const float CurveRadius = 0.5f;
    private const float HalfPi = MathF.PI * 0.5f;
    private const float DefaultCurveLength = MathF.PI * CurveRadius * 0.5f;
    private const float MovementEpsilon = 0.00001f;
    private const int MaxMovementIterations = 256;

    // ============================================================
    // TRAJECTORY HISTORY
    // ============================================================

    private readonly List<TrajectoryPoint> _trajectory = new();
    private float _totalTravelDistance;

    private readonly struct TrajectoryPoint
    {
        public readonly Vector2 Position;
        public readonly float Distance;

        public TrajectoryPoint(Vector2 position, float distance)
        {
            Position = position;
            Distance = distance;
        }
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public Train(Vector2 spawnPosition, TrackConnections initialDirection, float speed)
    {
        Id = Guid.NewGuid();
        Composition = new TrainComposition();
        Position = spawnPosition;
        Direction = initialDirection;
        _speed = speed;
        DistanceAlongTrack = 0f;
        TotalDistance = 0f;

        ResetCurveState();
        ResetTrajectory();
    }

    // ============================================================
    // MAP
    // ============================================================

    public void SetMap(GameMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public void Update(float deltaTime)
    {
        if (deltaTime <= 0.0f || !CanMove || _map is null || _speed <= 0.0f)
            return;

        float distance = _speed * deltaTime;
        if (distance <= MovementEpsilon)
            return;

        Move(distance);
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
        ResetCurveState();
        ResetTrajectory();
    }

    public void SetDirection(TrackConnections direction)
    {
        ValidateDirection(direction);
        Direction = direction;
        ResetCurveState();
        ResetTrajectory();
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
                ? MathF.Atan2(_trajectory[1].Position.Y - first.Position.Y, _trajectory[1].Position.X - first.Position.X)
                : GetDirectionAngle(Direction);
            return (first.Position, angle);
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

    private static float GetDirectionAngle(TrackConnections direction) => direction switch
    {
        TrackConnections.East => 0f,
        TrackConnections.South => MathHelper.PiOver2,
        TrackConnections.West => MathHelper.Pi,
        TrackConnections.North => -MathHelper.PiOver2,
        _ => 0f
    };

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
        if (Composition.Vehicles.Count == 0)
            return Position;

        int lastIndex = Composition.Vehicles.Count - 1;
        float distanceToLast = GetDistanceToVehicle(lastIndex);
        return GetPositionBehindHead(distanceToLast);
    }

    public TrackConnections GetLastVehicleDirection()
    {
        if (Composition.Vehicles.Count == 0)
            return Direction;

        int lastIndex = Composition.Vehicles.Count - 1;
        var transform = GetVehicleTransform(lastIndex);
        return VectorToDirection(transform.Rotation);
    }

    private static TrackConnections VectorToDirection(float angle)
    {
        while (angle > MathF.PI) angle -= MathF.Tau;
        while (angle < -MathF.PI) angle += MathF.Tau;

        float absAngle = MathF.Abs(angle);
        if (absAngle < MathF.PI / 4f || absAngle > 3f * MathF.PI / 4f)
        {
            return angle >= 0f ? TrackConnections.East : TrackConnections.West;
        }
        else
        {
            return angle >= 0f ? TrackConnections.South : TrackConnections.North;
        }
    }

    public Train Decouple(int startIndex)
    {
        if (startIndex <= 0 || startIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        float distanceToCut = GetDistanceToVehicle(startIndex);
        Vector2 cutPosition = GetPositionBehindHead(distanceToCut);
        TrackConnections cutDirection = Direction;

        var newTrain = new Train(cutPosition, cutDirection, _speed);
        if (_map != null)  // DODAJ TO SPRAWDZENIE
            newTrain.SetMap(_map);

        var vehiclesToMove = new List<Vehicle>();
        for (int i = startIndex; i < Composition.Vehicles.Count; i++)
        {
            vehiclesToMove.Add(Composition.Vehicles[i]);
        }

        foreach (var vehicle in vehiclesToMove)
        {
            Composition.RemoveVehicle(vehicle);
            newTrain.Composition.AddVehicle(vehicle);
        }

        newTrain.ResetTrajectory();
        TrainDecoupled?.Invoke(this, new TrainEventArgs(this, null, startIndex));

        return newTrain;
    }

    public void Couple(Train otherTrain)
    {
        if (otherTrain == null)
            throw new ArgumentNullException(nameof(otherTrain));

        if (otherTrain == this)
            throw new InvalidOperationException("Cannot couple train to itself");

        float distance = Vector2.Distance(this.Position, otherTrain.Position);
        if (distance > 2.0f)
        {
            System.Diagnostics.Debug.WriteLine("[TRAIN] Warning: Coupling trains that are far apart");
        }

        foreach (var vehicle in otherTrain.Composition.Vehicles)
        {
            this.Composition.AddVehicle(vehicle);
        }

        otherTrain.Composition.Clear();
        ResetTrajectory();
        TrainCoupled?.Invoke(this, new TrainEventArgs(this, null, 0));
    }

    public bool IsOnTrack()
    {
        if (_map is null) return false;
        var cell = GetCurrentCell();
        return _map.TryGetTrack(cell, out var track) && track != null;
    }

    // ============================================================
    // TRAJECTORY HISTORY
    // ============================================================

    private void ResetTrajectory()
    {
        _trajectory.Clear();
        _totalTravelDistance = 0f;
        _trajectory.Add(new TrajectoryPoint(Position, 0f));
    }

    private void AddTrajectoryPoint(Vector2 position, float travelledDistance)
    {
        if (travelledDistance > 0.0f)
        {
            _totalTravelDistance += travelledDistance;
        }

        if (_trajectory.Count > 0)
        {
            TrajectoryPoint last = _trajectory[_trajectory.Count - 1];
            if ((last.Position - position).LengthSquared() < MovementEpsilon * MovementEpsilon)
                return;
        }

        _trajectory.Add(new TrajectoryPoint(position, _totalTravelDistance));

        float requiredHistory = MathF.Max(Length * 5.0f, 15.0f);
        float minimumDistance = _totalTravelDistance - requiredHistory;

        while (_trajectory.Count > 2 && _trajectory[1].Distance < minimumDistance)
        {
            _trajectory.RemoveAt(0);
        }
    }

    // ============================================================
    // GRID MOVEMENT
    // ============================================================

    private void Move(float distance)
    {
        if (_map is null) return;

        if (Position.X < 0 || Position.X > _map.Size.Width ||
            Position.Y < 0 || Position.Y > _map.Size.Height)
        {
            System.Diagnostics.Debug.WriteLine($"[TRAIN] WARNING: Train out of bounds! {Position}");
            _speed = 0;
            return;
        }

        float remaining = distance;
        int iterations = 0;

        while (remaining > MovementEpsilon)
        {
            if (++iterations > MaxMovementIterations)
            {
                System.Diagnostics.Debug.WriteLine("[TRAIN] Movement iteration limit reached.");
                break;
            }

            if (_isOnCurve)
            {
                MoveOnCurve(ref remaining);
                continue;
            }

            MapPosition currentCell = GetCurrentCell();

            if (!_map.TryGetTrack(currentCell, out TrackCell? track) || track is null)
                break;

            if (track.Geometry == TrackGeometry.Curve)
            {
                if (!EnterCurve(track))
                    break;
                continue;
            }

            if (!track.HasConnection(Direction))
                break;

            float distanceToBoundary = GetDistanceToBoundary();

            if (distanceToBoundary <= MovementEpsilon)
            {
                if (!EnterNextCell())
                    break;
                continue;
            }

            float step = MathF.Min(remaining, distanceToBoundary);

            if (step <= MovementEpsilon)
            {
                if (!EnterNextCell())
                    break;
                continue;
            }

            MoveStraight(step);
            remaining -= step;

            if (distanceToBoundary - step <= MovementEpsilon)
            {
                if (!EnterNextCell())
                    break;
            }
        }
    }

    // ============================================================
    // STRAIGHT MOVEMENT
    // ============================================================

    private void MoveStraight(float distance)
    {
        if (distance <= 0.0f) return;

        Vector2 movement = DirectionToVector(Direction) * distance;
        Position += movement;
        TotalDistance += distance;
        DistanceAlongTrack += distance;
        AddTrajectoryPoint(Position, distance);
    }

    // ============================================================
    // NEXT CELL
    // ============================================================

    private bool EnterNextCell()
    {
        if (_map is null) return false;

        MapPosition currentCell = GetCurrentCell();
        MapPosition nextCell = GetNextCell(currentCell);

        if (!_map.TryGetTrack(nextCell, out TrackCell? nextTrack) || nextTrack is null)
            return false;

        TrackConnections entrySide = GetOppositeDirection(Direction);

        if (!nextTrack.HasConnection(entrySide))
            return false;

        Position = GetPositionAtEntry(nextCell, Direction);
        AddTrajectoryPoint(Position, 0.0f);

        if (nextTrack.Geometry == TrackGeometry.Straight)
            return true;

        if (nextTrack.Geometry == TrackGeometry.Curve)
            return EnterCurve(nextTrack);

        return false;
    }

    // ============================================================
    // CURVE ENTRY
    // ============================================================

    private bool EnterCurve(TrackCell track)
    {
        TrackConnections entrySide = GetOppositeDirection(Direction);

        if (!track.HasConnection(entrySide))
            return false;

        TrackConnections exitSide = GetCurveExitDirection(track.Connections, entrySide);

        if (exitSide == TrackConnections.None)
            return false;

        if (!IsPerpendicular(entrySide, exitSide))
            return false;

        _curveCell = track.Position;
        _curveEntrySide = entrySide;
        _curveExitSide = exitSide;
        _curveDistance = 0.0f;
        _curveLength = DefaultCurveLength;
        _isOnCurve = true;

        SetupArcParams(_curveCell, _curveEntrySide, _curveExitSide);

        Position = GetArcPosition(0.0f);
        AddTrajectoryPoint(Position, 0.0f);

        return true;
    }

    // ============================================================
    // CURVE MOVEMENT
    // ============================================================

    private void MoveOnCurve(ref float remaining)
    {
        if (!_isOnCurve) return;

        float remainingOnCurve = _curveLength - _curveDistance;

        if (remainingOnCurve <= MovementEpsilon)
        {
            FinishCurve();
            return;
        }

        float step = MathF.Min(remaining, remainingOnCurve);

        if (step <= MovementEpsilon)
        {
            FinishCurve();
            return;
        }

        float oldProgress = MathHelper.Clamp(_curveDistance / _curveLength, 0.0f, 1.0f);

        _curveDistance += step;
        remaining -= step;

        float newProgress = MathHelper.Clamp(_curveDistance / _curveLength, 0.0f, 1.0f);

        // Dodaj dodatkowy punkt w środku dla płynności
        if (step > 0.05f)
        {
            float midProgress = (oldProgress + newProgress) / 2f;
            if (midProgress > 0f && midProgress < 1f && midProgress != oldProgress && midProgress != newProgress)
            {
                Vector2 midPos = GetArcPosition(midProgress);
                float midDistance = step / 2f;
                _totalTravelDistance += midDistance;
                _trajectory.Add(new TrajectoryPoint(midPos, _totalTravelDistance));
            }
        }

        Position = GetArcPosition(newProgress);
        TotalDistance += step;
        DistanceAlongTrack += step;
        AddTrajectoryPoint(Position, step);

        if (_curveLength - _curveDistance <= MovementEpsilon)
        {
            FinishCurve();
        }
    }

    // ============================================================
    // ARC GEOMETRY
    // ============================================================

    private void SetupArcParams(MapPosition cell, TrackConnections entrySide, TrackConnections exitSide)
    {
        if ((entrySide == TrackConnections.West && exitSide == TrackConnections.North) ||
            (entrySide == TrackConnections.North && exitSide == TrackConnections.West))
        {
            _arcCenter = new Vector2(cell.X, cell.Y);
            if (entrySide == TrackConnections.West)
            {
                _arcStartAngle = MathF.PI;
                _arcSweepAngle = -HalfPi;
            }
            else
            {
                _arcStartAngle = HalfPi;
                _arcSweepAngle = HalfPi;
            }
            return;
        }

        if ((entrySide == TrackConnections.East && exitSide == TrackConnections.North) ||
            (entrySide == TrackConnections.North && exitSide == TrackConnections.East))
        {
            _arcCenter = new Vector2(cell.X + 1.0f, cell.Y);
            if (entrySide == TrackConnections.East)
            {
                _arcStartAngle = 0.0f;
                _arcSweepAngle = -HalfPi;
            }
            else
            {
                _arcStartAngle = -HalfPi;
                _arcSweepAngle = HalfPi;
            }
            return;
        }

        if ((entrySide == TrackConnections.East && exitSide == TrackConnections.South) ||
            (entrySide == TrackConnections.South && exitSide == TrackConnections.East))
        {
            _arcCenter = new Vector2(cell.X + 1.0f, cell.Y + 1.0f);
            if (entrySide == TrackConnections.East)
            {
                _arcStartAngle = MathF.PI;
                _arcSweepAngle = HalfPi;
            }
            else
            {
                _arcStartAngle = -HalfPi;
                _arcSweepAngle = -HalfPi;
            }
            return;
        }

        if ((entrySide == TrackConnections.West && exitSide == TrackConnections.South) ||
            (entrySide == TrackConnections.South && exitSide == TrackConnections.West))
        {
            _arcCenter = new Vector2(cell.X, cell.Y + 1.0f);
            if (entrySide == TrackConnections.West)
            {
                _arcStartAngle = 0.0f;
                _arcSweepAngle = HalfPi;
            }
            else
            {
                _arcStartAngle = HalfPi;
                _arcSweepAngle = HalfPi;
            }
            return;
        }

        throw new InvalidOperationException($"Unsupported curve: {entrySide} -> {exitSide}");
    }

    private Vector2 GetArcPosition(float progress)
    {
        progress = MathHelper.Clamp(progress, 0.0f, 1.0f);
        float angle = _arcStartAngle + (_arcSweepAngle * progress);

        return _arcCenter + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * CurveRadius;
    }

    // ============================================================
    // FINISH CURVE
    // ============================================================

    private void FinishCurve()
    {
        if (!_isOnCurve) return;

        Direction = _curveExitSide;
        MapPosition curveCell = _curveCell;
        MapPosition nextCell = GetNextCell(curveCell);

        if (_map != null &&
            _map.TryGetTrack(nextCell, out TrackCell? nextTrack) &&
            nextTrack is not null &&
            nextTrack.HasConnection(GetOppositeDirection(Direction)))
        {
            Position = GetPositionAtEntry(nextCell, Direction);
            AddTrajectoryPoint(Position, 0.0f);
        }
        else
        {
            Position = GetArcPosition(1.0f);
        }

        ResetCurveState();
    }

    // ============================================================
    // GRID TRANSITIONS
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

        return Direction switch
        {
            TrackConnections.East => MathF.Max(0.0f, (cell.X + 1.0f) - Position.X),
            TrackConnections.West => MathF.Max(0.0f, Position.X - cell.X),
            TrackConnections.South => MathF.Max(0.0f, (cell.Y + 1.0f) - Position.Y),
            TrackConnections.North => MathF.Max(0.0f, Position.Y - cell.Y),
            _ => 0.0f
        };
    }

    private static MapPosition GetNextCell(MapPosition cell, TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North => new MapPosition(cell.X, cell.Y - 1),
            TrackConnections.East => new MapPosition(cell.X + 1, cell.Y),
            TrackConnections.South => new MapPosition(cell.X, cell.Y + 1),
            TrackConnections.West => new MapPosition(cell.X - 1, cell.Y),
            _ => cell
        };
    }

    private MapPosition GetNextCell(MapPosition cell)
    {
        return GetNextCell(cell, Direction);
    }

    private static Vector2 GetPositionAtEntry(MapPosition cell, TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.East => new Vector2(cell.X + MovementEpsilon, cell.Y + 0.5f),
            TrackConnections.West => new Vector2(cell.X + 1.0f - MovementEpsilon, cell.Y + 0.5f),
            TrackConnections.South => new Vector2(cell.X + 0.5f, cell.Y + MovementEpsilon),
            TrackConnections.North => new Vector2(cell.X + 0.5f, cell.Y + 1.0f - MovementEpsilon),
            _ => new Vector2(cell.X + 0.5f, cell.Y + 0.5f)
        };
    }

    // ============================================================
    // CURVE HELPERS
    // ============================================================

    private static TrackConnections GetCurveExitDirection(TrackConnections connections, TrackConnections entrySide)
    {
        if (!connections.HasFlag(entrySide))
            return TrackConnections.None;

        TrackConnections exits = connections & ~entrySide;

        if (exits == TrackConnections.None)
            return TrackConnections.None;

        if (exits.HasFlag(TrackConnections.North)) return TrackConnections.North;
        if (exits.HasFlag(TrackConnections.East)) return TrackConnections.East;
        if (exits.HasFlag(TrackConnections.South)) return TrackConnections.South;
        if (exits.HasFlag(TrackConnections.West)) return TrackConnections.West;

        return TrackConnections.None;
    }

    private static bool IsPerpendicular(TrackConnections first, TrackConnections second)
    {
        bool firstHorizontal = first == TrackConnections.East || first == TrackConnections.West;
        bool secondHorizontal = second == TrackConnections.East || second == TrackConnections.West;
        return firstHorizontal != secondHorizontal;
    }

    // ============================================================
    // RENDERER ROTATION
    // ============================================================

    public float GetRotation()
    {
        Vector2 tangent;

        if (_isOnCurve && _curveLength > MovementEpsilon)
        {
            float progress = MathHelper.Clamp(_curveDistance / _curveLength, 0.0f, 1.0f);
            float angle = _arcStartAngle + (_arcSweepAngle * progress);

            tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));

            if (_arcSweepAngle < 0.0f)
                tangent = -tangent;
        }
        else
        {
            tangent = DirectionToVector(Direction);
        }

        return MathF.Atan2(tangent.Y, tangent.X);
    }

    // ============================================================
    // VEHICLE POSITIONS
    // ============================================================

    public List<Vector2> GetVehiclePositions(float vehicleSpacing = 1.0f)
    {
        var result = new List<Vector2>(Composition.Vehicles.Count);

        if (Composition.Vehicles.Count == 0)
            return result;

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
                float spacing = vehicleSpacing > MovementEpsilon ? vehicleSpacing : vehicle.Parameters.Length;
                result.Add(GetPositionBehindHead(distanceBehind));
                distanceBehind += spacing;
            }
        }

        return result;
    }

    public float GetVehicleDistance(int vehicleIndex)
    {
        return GetDistanceToVehicle(vehicleIndex);
    }

    // ============================================================
    // VEHICLE TRAJECTORY
    // ============================================================

    private Vector2 GetPositionBehindHead(float distanceBehind)
    {
        if (distanceBehind <= MovementEpsilon)
            return Position;

        float targetDistance = _totalTravelDistance - distanceBehind;

        if (_trajectory.Count == 0 || targetDistance <= 0.0f)
        {
            return Position - DirectionToVector(Direction) * distanceBehind;
        }

        for (int i = _trajectory.Count - 1; i > 0; i--)
        {
            TrajectoryPoint newer = _trajectory[i];
            TrajectoryPoint older = _trajectory[i - 1];

            if (targetDistance >= older.Distance && targetDistance <= newer.Distance)
            {
                float span = newer.Distance - older.Distance;
                if (span <= MovementEpsilon)
                    return older.Position;

                float t = (targetDistance - older.Distance) / span;
                return Vector2.Lerp(older.Position, newer.Position, t);
            }
        }

        return _trajectory[0].Position;
    }

    // ============================================================
    // CURVE STATE
    // ============================================================

    private void ResetCurveState()
    {
        _isOnCurve = false;
        _curveCell = new MapPosition(0, 0);
        _curveEntrySide = TrackConnections.None;
        _curveExitSide = TrackConnections.None;
        _arcCenter = Vector2.Zero;
        _arcStartAngle = 0.0f;
        _arcSweepAngle = 0.0f;
        _curveDistance = 0.0f;
        _curveLength = 0.0f;
    }

    // ============================================================
    // DIRECTION HELPERS
    // ============================================================

    private static Vector2 DirectionToVector(TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North => new Vector2(0.0f, -1.0f),
            TrackConnections.East => new Vector2(1.0f, 0.0f),
            TrackConnections.South => new Vector2(0.0f, 1.0f),
            TrackConnections.West => new Vector2(-1.0f, 0.0f),
            _ => throw new ArgumentException("Direction must contain exactly one cardinal direction.", nameof(direction))
        };
    }

    private static TrackConnections GetOppositeDirection(TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North => TrackConnections.South,
            TrackConnections.East => TrackConnections.West,
            TrackConnections.South => TrackConnections.North,
            TrackConnections.West => TrackConnections.East,
            _ => TrackConnections.None
        };
    }

    private static void ValidateDirection(TrackConnections direction)
    {
        if (direction != TrackConnections.North &&
            direction != TrackConnections.East &&
            direction != TrackConnections.South &&
            direction != TrackConnections.West)
        {
            throw new ArgumentException("Train direction must be a single cardinal direction.", nameof(direction));
        }
    }
}

public static class MathHelper
{
    public const float PiOver2 = MathF.PI / 2.0f;
    public const float Pi = MathF.PI;

    public static float Clamp(float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }
}