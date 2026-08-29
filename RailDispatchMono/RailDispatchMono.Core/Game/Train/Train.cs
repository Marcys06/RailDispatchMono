using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public Train(
    Vector2 spawnPosition,
    TrackConnections initialDirection,
    float speed,
    IEnumerable<Vehicle> vehicles)  // <- DODAJ
    {
        Id = Guid.NewGuid();
        Position = spawnPosition;
        Direction = initialDirection;
        _speed = speed;

        // ✅ Użyj przekazanych pojazdów
        Composition = new TrainComposition();
        foreach (var vehicle in vehicles)
        {
            Composition.AddVehicle(vehicle);
        }

        // ✅ Ustaw maxSpeed na podstawie pojazdów
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
        if (deltaTime <= 0.0f || !CanMove || _map is null)
            return;

        System.Diagnostics.Debug.WriteLine(
       $"[TRAIN] 🟢 START - Pos: ({Position.X:F4}, {Position.Y:F4}), " +
       $"Dir: {Direction}, Speed: {Speed:F2} m/s ({Speed * 3.6f:F1} km/h)"
   );

        // 1. Znajdź sygnał
        var nextSignal = GetNextSignal();
        if (nextSignal != null)
        {
            _lastSignal = nextSignal;
            _lastSignalSpeed = GetSpeedFromSignal(nextSignal);
        }

        _targetSpeed = _lastSignalSpeed;

        // 2. ✅ POBIERZ PARAMETRY Z POJAZDÓW
        float maxSpeed = float.MaxValue;
        float maxAcceleration = 0f;
        float maxBraking = 0f;

        foreach (var vehicle in Composition.Vehicles)
        {
            var p = vehicle.Parameters;
            if (p.MaxSpeed < maxSpeed)
                maxSpeed = p.MaxSpeed;
            if (p.Acceleration > maxAcceleration)
                maxAcceleration = p.Acceleration;
            if (p.Braking > maxBraking)
                maxBraking = p.Braking;
        }

        // Użyj maxBraking z pojazdów
        float decelerationRate = maxBraking > 0 ? maxBraking : 20.0f; // fallback

        // 3. Zmień prędkość używając parametrów z pojazdów
        if (Speed < _targetSpeed)
        {
            Speed = Math.Min(
                Speed + maxAcceleration * deltaTime,
                Math.Min(_targetSpeed, maxSpeed)
            );
        }
        else if (Speed > _targetSpeed)
        {
            Speed = Math.Max(
                Speed - decelerationRate * deltaTime,
                _targetSpeed
            );
        }

        // 4. Wykonaj ruch
        float distance = Speed * deltaTime;
        if (distance > MovementEpsilon)
            Move(distance);
    }

    // ============================================================
    // PUBLIC POSITION API
    // ============================================================

    public Vector2 GetHeadPosition()
    {
        return Position;
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;

        DistanceAlongTrack = 0f;
        TotalDistance = 0f;

        // ✅ Resetuj ostatni sygnał przy ręcznym ustawieniu pozycji
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;

        ResetCurveState();
        ResetTrajectory();
    }

    public void SetDirection(TrackConnections direction)
    {
        ValidateDirection(direction);

        Direction = direction;

        // ✅ Resetuj ostatni sygnał przy zmianie kierunku
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;

        ResetCurveState();
        ResetTrajectory();
    }

    // ============================================================
    // VEHICLE TRANSFORM
    // ============================================================

    public (Vector2 Position, float Rotation) GetVehicleTransform(
        int vehicleIndex)
    {
        if (vehicleIndex < 0 ||
            vehicleIndex >= Composition.Vehicles.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));
        }

        float distanceBehindHead =
            GetDistanceToVehicle(vehicleIndex);

        float targetDistance =
            _totalTravelDistance - distanceBehindHead;

        if (_trajectory.Count == 0)
        {
            return (
                Position,
                GetDirectionAngle(Direction));
        }

        if (targetDistance <= _trajectory[0].Distance)
        {
            var first = _trajectory[0];

            var angle = _trajectory.Count > 1
                ? MathF.Atan2(
                    _trajectory[1].Position.Y - first.Position.Y,
                    _trajectory[1].Position.X - first.Position.X)
                : GetDirectionAngle(Direction);

            return (
                first.Position,
                angle);
        }

        if (targetDistance >=
            _trajectory[_trajectory.Count - 1].Distance)
        {
            var last =
                _trajectory[_trajectory.Count - 1];

            var angle = _trajectory.Count > 1
                ? MathF.Atan2(
                    last.Position.Y -
                    _trajectory[_trajectory.Count - 2].Position.Y,

                    last.Position.X -
                    _trajectory[_trajectory.Count - 2].Position.X)
                : GetDirectionAngle(Direction);

            return (
                last.Position,
                angle);
        }

        for (int i = _trajectory.Count - 1; i > 0; i--)
        {
            var curr = _trajectory[i];
            var prev = _trajectory[i - 1];

            if (targetDistance >= prev.Distance &&
                targetDistance <= curr.Distance)
            {
                float segmentLength =
                    curr.Distance - prev.Distance;

                float t = segmentLength > MovementEpsilon
                    ? (targetDistance - prev.Distance) /
                      segmentLength
                    : 0f;

                Vector2 pos =
                    Vector2.Lerp(
                        prev.Position,
                        curr.Position,
                        t);

                Vector2 dir =
                    curr.Position - prev.Position;

                float angle =
                    dir != Vector2.Zero
                        ? MathF.Atan2(dir.Y, dir.X)
                        : GetDirectionAngle(Direction);

                return (
                    pos,
                    angle);
            }
        }

        return (
            Position,
            GetDirectionAngle(Direction));
    }

    private static float GetDirectionAngle(
        TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.East => 0f,
            TrackConnections.South => MathHelper.PiOver2,
            TrackConnections.West => MathHelper.Pi,
            TrackConnections.North => -MathHelper.PiOver2,
            _ => 0f
        };
    }

    // ============================================================
    // VEHICLE MANAGEMENT
    // ============================================================

    public float GetDistanceToVehicle(int vehicleIndex)
    {
        if (vehicleIndex < 0 ||
            vehicleIndex >= Composition.Vehicles.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleIndex));
        }

        float distance = 0f;

        for (int i = 0; i < vehicleIndex; i++)
        {
            distance +=
                Composition.Vehicles[i].Parameters.Length;
        }

        distance +=
            Composition.Vehicles[vehicleIndex]
                .Parameters.Length * 0.5f;

        return distance;
    }

    public float GetTotalTrainLength()
    {
        return Length;
    }

    public Vector2 GetLastVehiclePosition()
    {
        if (Composition.Vehicles.Count == 0)
            return Position;

        int lastIndex =
            Composition.Vehicles.Count - 1;

        float distanceToLast =
            GetDistanceToVehicle(lastIndex);

        return GetPositionBehindHead(distanceToLast);
    }

    public TrackConnections GetLastVehicleDirection()
    {
        if (Composition.Vehicles.Count == 0)
            return Direction;

        int lastIndex =
            Composition.Vehicles.Count - 1;

        var transform =
            GetVehicleTransform(lastIndex);

        return VectorToDirection(transform.Rotation);
    }

    private static TrackConnections VectorToDirection(
        float angle)
    {
        while (angle > MathF.PI)
            angle -= MathF.Tau;

        while (angle < -MathF.PI)
            angle += MathF.Tau;

        float absAngle = MathF.Abs(angle);

        if (absAngle < MathF.PI / 4f ||
            absAngle > 3f * MathF.PI / 4f)
        {
            return angle >= 0f
                ? TrackConnections.East
                : TrackConnections.West;
        }

        return angle >= 0f
            ? TrackConnections.South
            : TrackConnections.North;
    }
    /*
    public Train Decouple(int startIndex)
    {
        if (startIndex <= 0 ||
            startIndex >= Composition.Vehicles.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startIndex));
        }

        float distanceToCut =
            GetDistanceToVehicle(startIndex);

        Vector2 cutPosition =
            GetPositionBehindHead(distanceToCut);

        TrackConnections cutDirection =
            Direction;

        var newTrain =
            new Train(
                cutPosition,
                cutDirection,
                _speed);

        if (_map != null)
            newTrain.SetMap(_map);

        var vehiclesToMove =
            new List<Vehicle>();

        for (
            int i = startIndex;
            i < Composition.Vehicles.Count;
            i++)
        {
            vehiclesToMove.Add(
                Composition.Vehicles[i]);
        }

        foreach (var vehicle in vehiclesToMove)
        {
            Composition.RemoveVehicle(vehicle);
            newTrain.Composition.AddVehicle(vehicle);
        }

        newTrain.ResetTrajectory();

        TrainDecoupled?.Invoke(
            this,
            new TrainEventArgs(
                this,
                null,
                startIndex));

        return newTrain;
    }

    public void Couple(Train otherTrain)
    {
        if (otherTrain == null)
            throw new ArgumentNullException(
                nameof(otherTrain));

        if (otherTrain == this)
            throw new InvalidOperationException(
                "Cannot couple train to itself");

        float distance =
            Vector2.Distance(
                this.Position,
                otherTrain.Position);

        if (distance > 2.0f)
        {
            System.Diagnostics.Debug.WriteLine(
                "[TRAIN] Warning: Coupling trains that are far apart");
        }

        foreach (var vehicle in
                 otherTrain.Composition.Vehicles)
        {
            this.Composition.AddVehicle(vehicle);
        }

        otherTrain.Composition.Clear();

        ResetTrajectory();

        TrainCoupled?.Invoke(
            this,
            new TrainEventArgs(
                this,
                null,
                0));
    }
    */

    public Train(
    Vector2 spawnPosition,
    TrackConnections initialDirection,
    float speed)
    : this(spawnPosition, initialDirection, speed, Array.Empty<Vehicle>())
    {
        // Pusty - wszystkie pojazdy będą dodane później
    }

    public bool IsOnTrack()
    {
        if (_map is null)
            return false;

        var cell = GetCurrentCell();

        return _map.TryGetTrack(
            cell,
            out var track) &&
               track != null;
    }

    // ============================================================
    // DEBUGGER HELPERS
    // ============================================================

    public IReadOnlyList<(Vector2 Position, float Distance)>
        GetTrajectoryHistory()
    {
        var result =
            new List<(Vector2 Position, float Distance)>(
                _trajectory.Count);

        foreach (var point in _trajectory)
        {
            result.Add(
                (point.Position, point.Distance));
        }

        return result;
    }

    public int TrajectoryPointCount =>
        _trajectory.Count;

    public (Vector2 Position, float Distance)?
        GetLastTrajectoryPoint()
    {
        if (_trajectory.Count == 0)
            return null;

        var last =
            _trajectory[_trajectory.Count - 1];

        return (
            last.Position,
            last.Distance);
    }

    // ============================================================
    // TRAJECTORY HISTORY
    // ============================================================

    private void ResetTrajectory()
    {
        _trajectory.Clear();

        _totalTravelDistance = 0f;

        _trajectory.Add(
            new TrajectoryPoint(
                Position,
                0f));
    }

    private void AddTrajectoryPoint(
        Vector2 position,
        float travelledDistance)
    {
        if (travelledDistance > 0.0f)
        {
            _totalTravelDistance +=
                travelledDistance;
        }

        if (_trajectory.Count > 0)
        {
            TrajectoryPoint last =
                _trajectory[_trajectory.Count - 1];

            if (
                (last.Position - position)
                    .LengthSquared()
                <
                MovementEpsilon *
                MovementEpsilon)
            {
                return;
            }
        }

        _trajectory.Add(
            new TrajectoryPoint(
                position,
                _totalTravelDistance));

        float requiredHistory =
            MathF.Max(
                Length * 25.0f,
                60.0f);

        float minimumDistance =
            _totalTravelDistance -
            requiredHistory;

        while (
            _trajectory.Count > 2 &&
            _trajectory[1].Distance <
            minimumDistance)
        {
            _trajectory.RemoveAt(0);
        }
    }

    // ============================================================
    // GRID MOVEMENT
    // ============================================================

    private void Move(float distance)
    {
        if (_map is null)
            return;

        if (
            Position.X < 0 ||
            Position.X > _map.Size.Width ||
            Position.Y < 0 ||
            Position.Y > _map.Size.Height)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[TRAIN] WARNING: Train out of bounds! {Position}");

            _speed = 0;
            return;
        }

        float remaining = distance;

        int iterations = 0;

        MapPosition lastCell =
            GetCurrentCell();

        int sameCellCount = 0;

        while (remaining > MovementEpsilon)
        {
            if (++iterations >
                MaxMovementIterations)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[TRAIN] Movement iteration limit reached.");

                break;
            }

            MapPosition currentCell =
                GetCurrentCell();

            if (currentCell == lastCell)
            {
                sameCellCount++;

                if (sameCellCount > 10)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[TRAIN] Stuck in cell {currentCell} - forcing exit");

                    if (!EnterNextCell())
                        break;

                    sameCellCount = 0;

                    continue;
                }
            }
            else
            {
                sameCellCount = 0;
                lastCell = currentCell;
            }

            if (_isOnCurve)
            {
                MoveOnCurve(ref remaining);
                continue;
            }

            if (
                !_map.TryGetTrack(
                    currentCell,
                    out TrackCell? track) ||
                track is null)
            {
                break;
            }

            TrackConnections entrySide =
                GetOppositeDirection(Direction);

            bool isCurve =
                track.Geometry ==
                TrackGeometry.Curve;

            TrackConnections exitSide =
                track.Geometry ==
                TrackGeometry.Junction
                    ? track.GetExitDirection(entrySide)
                    : GetCurveExitDirection(
                        track.Connections,
                        entrySide);

            bool isTurning =
                exitSide != TrackConnections.None &&
                IsPerpendicular(
                    entrySide,
                    exitSide);

            if (
                isCurve ||
                (
                    track.Geometry ==
                    TrackGeometry.Junction &&
                    isTurning
                ))
            {
                if (!EnterCurve(track))
                    break;

                continue;
            }

            if (!track.HasConnection(Direction))
                break;

            float distanceToBoundary =
                GetDistanceToBoundary();

            if (distanceToBoundary <= MovementEpsilon)
            {
                if (!EnterNextCell())
                    break;

                continue;
            }

            float step =
                MathF.Min(
                    remaining,
                    distanceToBoundary);

            if (step < MovementEpsilon)
                step = MovementEpsilon;

            if (
                step >
                distanceToBoundary +
                MovementEpsilon)
            {
                step =
                    distanceToBoundary;
            }

            MoveStraight(step);

            remaining -= step;

            float newDistanceToBoundary =
                GetDistanceToBoundary();

            if (
                newDistanceToBoundary <=
                MovementEpsilon)
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
        if (distance <= 0.0f)
            return;

        Vector2 oldPos = Position;

        Vector2 movement =
            DirectionToVector(Direction) *
            distance;

        Vector2 newPos =
            oldPos + movement;

        MapPosition oldCell =
            GetCurrentCellFromPosition(oldPos);

        MapPosition newCell =
            GetCurrentCellFromPosition(newPos);

        if (
            newCell != oldCell &&
            newCell != GetNextCell(oldCell))
        {
            System.Diagnostics.Debug.WriteLine(
                $"[STRAIGHT] WARNING: Moving through multiple cells! " +
                $"{oldCell} -> {newCell}");

            newPos =
                GetPositionAtEntry(
                    newCell,
                    Direction);
        }

        Position = newPos;

        System.Diagnostics.Debug.WriteLine(
            $"[STRAIGHT] Dir:{Direction} " +
            $"Dist:{distance:F6} " +
            $"Old:({oldPos.X:F4},{oldPos.Y:F4}) " +
            $"New:({Position.X:F4},{Position.Y:F4})");

        TotalDistance += distance;

        DistanceAlongTrack += distance;

        AddTrajectoryPoint(
            Position,
            distance);
    }

    private MapPosition GetCurrentCellFromPosition(
        Vector2 pos)
    {
        return new MapPosition(
            (int)MathF.Floor(pos.X),
            (int)MathF.Floor(pos.Y));
    }

    // ============================================================
    // NEXT CELL
    // ============================================================

    private bool EnterNextCell()
    {
        if (_map is null)
            return false;

        MapPosition currentCell =
            GetCurrentCell();

        MapPosition nextCell =
            GetNextCell(currentCell);

        System.Diagnostics.Debug.WriteLine(
            $"[ENTER] Current:{currentCell} " +
            $"Next:{nextCell} " +
            $"Dir:{Direction} " +
            $"Pos:{Position}");

        if (currentCell == nextCell)
            return false;

        if (
            !_map.TryGetTrack(
                nextCell,
                out TrackCell? nextTrack) ||
            nextTrack is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ENTER] No track at {nextCell}");

            return false;
        }

        TrackConnections entrySide =
            GetOppositeDirection(Direction);

        if (!nextTrack.HasConnection(entrySide))
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ENTER] No connection {entrySide} at {nextCell}");

            return false;
        }

        TrackConnections exitSide =
            nextTrack.GetExitDirection(entrySide);

        if (exitSide == TrackConnections.None)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[ENTER] No exit path available from " +
                $"{entrySide} at {nextCell}");

            return false;
        }

        if (IsPerpendicular(
                entrySide,
                exitSide))
        {
            return EnterCurve(
                nextTrack,
                entrySide,
                exitSide);
        }

        // ========================================================
        // PROSTY TOR
        // ========================================================

        Direction = exitSide;

        Vector2 entryPos =
            Direction switch
            {
                TrackConnections.East =>
                    new Vector2(
                        nextCell.X +
                        MovementEpsilon,
                        nextCell.Y + 0.5f),

                TrackConnections.West =>
                    new Vector2(
                        nextCell.X +
                        1.0f -
                        MovementEpsilon,
                        nextCell.Y + 0.5f),

                TrackConnections.South =>
                    new Vector2(
                        nextCell.X + 0.5f,
                        nextCell.Y +
                        MovementEpsilon),

                TrackConnections.North =>
                    new Vector2(
                        nextCell.X + 0.5f,
                        nextCell.Y +
                        1.0f -
                        MovementEpsilon),

                _ =>
                    new Vector2(
                        nextCell.X + 0.5f,
                        nextCell.Y + 0.5f)
            };

        Vector2 oldPos = Position;

        float actualDistance =
            Vector2.Distance(
                oldPos,
                entryPos);

        System.Diagnostics.Debug.WriteLine(
            $"[ENTER] OldPos:{oldPos} " +
            $"NewPos:{entryPos} " +
            $"Distance:{actualDistance:F6}");

        if (actualDistance >
            MovementEpsilon)
        {
            AddTrajectoryPoint(
                entryPos,
                actualDistance);

            TotalDistance +=
                actualDistance;

            DistanceAlongTrack +=
                actualDistance;
        }

        Position = entryPos;

        return true;
    }

    // ============================================================
    // CURVE ENTRY
    // ============================================================

    // ============================================================
    // CURVE ENTRY - Z 3 PARAMETRAMI
    // ============================================================

    



    private bool EnterCurve(TrackCell track)
    {
        TrackConnections entrySide = GetOppositeDirection(Direction);

        if (!track.HasConnection(entrySide))
            return false;

        TrackConnections exitSide = track.Geometry == TrackGeometry.Junction
            ? track.GetExitDirection(entrySide)
            : GetCurveExitDirection(track.Connections, entrySide);

        if (exitSide == TrackConnections.None)
            return false;

        if (!IsPerpendicular(entrySide, exitSide))
            return false;

        // ✅ TERAZ WYWOŁUJE METODĘ KTÓRA ISTNIEJE!
        return EnterCurve(track, entrySide, exitSide);
    }

    private bool EnterCurve(
        TrackCell track,
        TrackConnections entrySide,
        TrackConnections exitSide)
    {
        // ========================================================
        // POPRAWKA:
        // TrackCell posiada własną pozycję.
        // Nie wyliczamy jej ponownie przez GetCurrentCell().
        // ========================================================

        MapPosition cell = track.Position;

        Direction = exitSide;

        _curveCell = cell;

        _curveEntrySide = entrySide;
        _curveExitSide = exitSide;

        // ========================================================
        // POPRAWKA:
        // Parametry łuku muszą zostać przygotowane przed
        // pierwszym użyciem GetArcPosition().
        // ========================================================

        SetupArcParams(
            cell,
            entrySide,
            exitSide);

        // ========================================================
        // POPRAWKA:
        // Ustawiamy długość oraz stan ruchu po łuku.
        // ========================================================

        _curveLength =
            DefaultCurveLength;

        _curveDistance = 0.0f;

        _isOnCurve = true;

        System.Diagnostics.Debug.WriteLine(
            $"[CURVE] Enter cell:{cell} " +
            $"Entry:{entrySide} " +
            $"Exit:{exitSide} " +
            $"Center:{_arcCenter} " +
            $"Start:{_arcStartAngle:F4} " +
            $"Sweep:{_arcSweepAngle:F4} " +
            $"Length:{_curveLength:F4}");

        return true;
    }

    // ============================================================
    // CURVE MOVEMENT
    // ============================================================

    private void MoveOnCurve(
        ref float remaining)
    {
        if (!_isOnCurve)
            return;

        float remainingOnCurve =
            _curveLength -
            _curveDistance;

        if (remainingOnCurve <=
            MovementEpsilon)
        {
            FinishCurve();
            return;
        }

        float step =
            MathF.Min(
                remaining,
                remainingOnCurve);

        if (step <= MovementEpsilon)
        {
            FinishCurve();
            return;
        }

        _curveDistance += step;

        remaining -= step;

        float progress =
            MathHelper.Clamp(
                _curveDistance /
                _curveLength,
                0.0f,
                1.0f);

        Position =
            GetArcPosition(progress);

        TotalDistance += step;

        DistanceAlongTrack += step;

        AddTrajectoryPoint(
            Position,
            step);

        if (
            _curveDistance >=
            _curveLength -
            MovementEpsilon)
        {
            _curveDistance =
                _curveLength;

            Position =
                GetArcPosition(1.0f);

            FinishCurve();
        }
    }

    // ============================================================
    // ARC GEOMETRY
    // ============================================================

    private void SetupArcParams(
        MapPosition cell,
        TrackConnections entrySide,
        TrackConnections exitSide)
    {
        float x = cell.X;
        float y = cell.Y;

        // WEST -> NORTH
        if (
            entrySide ==
                TrackConnections.West &&
            exitSide ==
                TrackConnections.North)
        {
            _arcCenter =
                new Vector2(
                    x,
                    y);

            _arcStartAngle = 0.0f;
            _arcSweepAngle = -HalfPi;

            return;
        }

        // NORTH -> WEST
        if (
            entrySide ==
                TrackConnections.North &&
            exitSide ==
                TrackConnections.West)
        {
            _arcCenter =
                new Vector2(
                    x,
                    y);

            _arcStartAngle = 0.0f;
            _arcSweepAngle = HalfPi;

            return;
        }

        // EAST -> NORTH
        if (
            entrySide ==
                TrackConnections.East &&
            exitSide ==
                TrackConnections.North)
        {
            _arcCenter =
                new Vector2(
                    x + 1.0f,
                    y);

            _arcStartAngle = HalfPi;
            _arcSweepAngle = HalfPi;

            return;
        }

        // NORTH -> EAST
        if (
            entrySide ==
                TrackConnections.North &&
            exitSide ==
                TrackConnections.East)
        {
            _arcCenter =
                new Vector2(
                    x + 1.0f,
                    y);

            _arcStartAngle =
                MathF.PI;

            _arcSweepAngle =
                -HalfPi;

            return;
        }

        // EAST -> SOUTH
        if (
            entrySide ==
                TrackConnections.East &&
            exitSide ==
                TrackConnections.South)
        {
            _arcCenter =
                new Vector2(
                    x + 1.0f,
                    y + 1.0f);

            _arcStartAngle =
                -HalfPi;

            _arcSweepAngle =
                -HalfPi;

            return;
        }

        // SOUTH -> EAST
        if (
            entrySide ==
                TrackConnections.South &&
            exitSide ==
                TrackConnections.East)
        {
            _arcCenter =
                new Vector2(
                    x + 1.0f,
                    y + 1.0f);

            _arcStartAngle =
                MathF.PI;

            _arcSweepAngle =
                HalfPi;

            return;
        }

        // WEST -> SOUTH
        if (
            entrySide ==
                TrackConnections.West &&
            exitSide ==
                TrackConnections.South)
        {
            _arcCenter =
                new Vector2(
                    x,
                    y + 1.0f);

            _arcStartAngle =
                -HalfPi;

            _arcSweepAngle =
                HalfPi;

            return;
        }

        // SOUTH -> WEST
        if (
            entrySide ==
                TrackConnections.South &&
            exitSide ==
                TrackConnections.West)
        {
            _arcCenter =
                new Vector2(
                    x,
                    y + 1.0f);

            _arcStartAngle = 0.0f;

            _arcSweepAngle =
                -HalfPi;

            return;
        }

        throw new InvalidOperationException(
            $"Unsupported curve: " +
            $"{entrySide} -> {exitSide}");
    }

    private Vector2 GetArcPosition(
        float progress)
    {
        progress =
            MathHelper.Clamp(
                progress,
                0.0f,
                1.0f);

        float angle =
            _arcStartAngle +
            (_arcSweepAngle * progress);

        return _arcCenter +
               new Vector2(
                   MathF.Cos(angle),
                   MathF.Sin(angle)) *
               CurveRadius;
    }

    // ============================================================
    // FINISH CURVE
    // ============================================================

    private void FinishCurve()
    {
        if (!_isOnCurve)
            return;

        // ========================================================
        // ZAPISUJEMY STAN PRZED ResetCurveState()
        // ========================================================

        MapPosition savedCurveCell =
            _curveCell;

        TrackConnections savedExitSide =
            _curveExitSide;

        float savedCurveDistance =
            _curveDistance;

        // ========================================================
        // Ustawiamy dokładnie końcową pozycję łuku.
        // ========================================================

        Position =
            GetArcPosition(1.0f);

        // ========================================================
        // POPRAWKA:
        // Kierunek po łuku pochodzi z _curveExitSide.
        // ========================================================

        Direction =
            _curveExitSide;

        System.Diagnostics.Debug.WriteLine(
            $"[FINISH CURVE] " +
            $"Cell:{savedCurveCell} " +
            $"Position:{Position} " +
            $"Exit:{savedExitSide} " +
            $"CurveDistance:{savedCurveDistance:F6}");

        // ========================================================
        // Resetujemy stan łuku.
        // ========================================================

        ResetCurveState();

        if (_map is null)
            return;

        // ========================================================
        // POPRAWKA:
        // Następne pole wyznaczamy na podstawie kierunku wyjścia
        // z łuku, a nie bieżącego Direction po potencjalnym
        // resecie.
        // ========================================================

        MapPosition nextCell =
            GetNextCell(
                savedCurveCell,
                savedExitSide);

        if (
            _map.TryGetTrack(
                nextCell,
                out TrackCell? nextTrack) &&
            nextTrack != null)
        {
            TrackConnections entrySide =
                GetOppositeDirection(
                    savedExitSide);

            if (
                nextTrack.HasConnection(
                    entrySide))
            {
                Vector2 oldPos =
                    Position;

                Vector2 entryPos =
                    GetPositionAtEntry(
                        nextCell,
                        savedExitSide);

                float transitionDistance =
                    Vector2.Distance(
                        oldPos,
                        entryPos);

                if (
                    transitionDistance >
                    MovementEpsilon)
                {
                    AddTrajectoryPoint(
                        entryPos,
                        transitionDistance);

                    TotalDistance +=
                        transitionDistance;

                    DistanceAlongTrack +=
                        transitionDistance;
                }

                Position =
                    entryPos;

                System.Diagnostics.Debug.WriteLine(
                    $"[FINISH CURVE] " +
                    $"Entered {nextCell}, " +
                    $"curve distance: " +
                    $"{savedCurveDistance:F6}, " +
                    $"transition: " +
                    $"{transitionDistance:F6}");
            }
        }
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
        MapPosition cell =
            GetCurrentCell();

        float result =
            Direction switch
            {
                TrackConnections.East =>
                    (cell.X + 1.0f) -
                    Position.X,

                TrackConnections.West =>
                    Position.X -
                    cell.X,

                TrackConnections.South =>
                    (cell.Y + 1.0f) -
                    Position.Y,

                TrackConnections.North =>
                    Position.Y -
                    cell.Y,

                _ => 0.0f
            };

        if (result < 0.0f)
            result = 0.0f;

        if (
            result < MovementEpsilon &&
            result > 0)
        {
            result =
                MovementEpsilon;
        }

        System.Diagnostics.Debug.WriteLine(
            $"[BOUNDARY] " +
            $"Cell:{cell} " +
            $"Dir:{Direction} " +
            $"Pos:{Position} " +
            $"Result:{result:F6}");

        return result;
    }

    private static MapPosition GetNextCell(
        MapPosition cell,
        TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North =>
                new MapPosition(
                    cell.X,
                    cell.Y - 1),

            TrackConnections.East =>
                new MapPosition(
                    cell.X + 1,
                    cell.Y),

            TrackConnections.South =>
                new MapPosition(
                    cell.X,
                    cell.Y + 1),

            TrackConnections.West =>
                new MapPosition(
                    cell.X - 1,
                    cell.Y),

            _ => cell
        };
    }

    private MapPosition GetNextCell(
        MapPosition cell)
    {
        return GetNextCell(
            cell,
            Direction);
    }

    private static Vector2 GetPositionAtEntry(
        MapPosition cell,
        TrackConnections direction)
    {
        const float epsilon =
            0.0001f;

        return direction switch
        {
            TrackConnections.East =>
                new Vector2(
                    cell.X + epsilon,
                    cell.Y + 0.5f),

            TrackConnections.West =>
                new Vector2(
                    cell.X + 1.0f - epsilon,
                    cell.Y + 0.5f),

            TrackConnections.South =>
                new Vector2(
                    cell.X + 0.5f,
                    cell.Y + epsilon),

            TrackConnections.North =>
                new Vector2(
                    cell.X + 0.5f,
                    cell.Y + 1.0f - epsilon),

            _ =>
                new Vector2(
                    cell.X + 0.5f,
                    cell.Y + 0.5f)
        };
    }

    // ============================================================
    // CURVE HELPERS
    // ============================================================

    private static TrackConnections GetCurveExitDirection(
        TrackConnections connections,
        TrackConnections entrySide)
    {
        if (!connections.HasFlag(entrySide))
            return TrackConnections.None;

        TrackConnections exits =
            connections & ~entrySide;

        if (exits ==
            TrackConnections.None)
        {
            return TrackConnections.None;
        }

        if (exits.HasFlag(
                TrackConnections.North))
        {
            return TrackConnections.North;
        }

        if (exits.HasFlag(
                TrackConnections.East))
        {
            return TrackConnections.East;
        }

        if (exits.HasFlag(
                TrackConnections.South))
        {
            return TrackConnections.South;
        }

        if (exits.HasFlag(
                TrackConnections.West))
        {
            return TrackConnections.West;
        }

        return TrackConnections.None;
    }

    private static bool IsPerpendicular(
        TrackConnections first,
        TrackConnections second)
    {
        bool firstHorizontal =
            first == TrackConnections.East ||
            first == TrackConnections.West;

        bool secondHorizontal =
            second == TrackConnections.East ||
            second == TrackConnections.West;

        return firstHorizontal !=
               secondHorizontal;
    }

    // ============================================================
    // RENDERER ROTATION
    // ============================================================

    public float GetRotation()
    {
        Vector2 tangent;

        if (
            _isOnCurve &&
            _curveLength > MovementEpsilon)
        {
            float progress =
                MathHelper.Clamp(
                    _curveDistance /
                    _curveLength,
                    0.0f,
                    1.0f);

            float angle =
                _arcStartAngle +
                (_arcSweepAngle * progress);

            tangent =
                new Vector2(
                    -MathF.Sin(angle),
                    MathF.Cos(angle));

            if (_arcSweepAngle < 0.0f)
                tangent = -tangent;
        }
        else
        {
            tangent =
                DirectionToVector(
                    Direction);
        }

        return MathF.Atan2(
            tangent.Y,
            tangent.X);
    }

    // ============================================================
    // VEHICLE POSITIONS
    // ============================================================

    public List<Vector2> GetVehiclePositions(
        float vehicleSpacing = 1.0f)
    {
        var result =
            new List<Vector2>(
                Composition.Vehicles.Count);

        if (Composition.Vehicles.Count == 0)
            return result;

        float distanceBehind = 0.0f;

        for (
            int i = 0;
            i < Composition.Vehicles.Count;
            i++)
        {
            var vehicle =
                Composition.Vehicles[i];

            if (i == 0)
            {
                result.Add(Position);

                distanceBehind =
                    vehicle.Parameters.Length;
            }
            else
            {
                float spacing =
                    vehicleSpacing >
                    MovementEpsilon
                        ? vehicleSpacing
                        : vehicle.Parameters.Length;

                result.Add(
                    GetPositionBehindHead(
                        distanceBehind));

                distanceBehind +=
                    spacing;
            }
        }

        return result;
    }

    public float GetVehicleDistance(
        int vehicleIndex)
    {
        return GetDistanceToVehicle(
            vehicleIndex);
    }
    // ============================================================
    // SIGNAL CONTROL
    // ============================================================

    private SignalController? _signalController;
    private float _targetSpeed;
    private float _maxSpeed = 160f / 3.6f;  // 160 km/h na m/s

    // ✅ OSTATNI WIDZIANY SYGNAŁ (obowiązuje aż do następnego)
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
        if (_signalController == null)
            return null;

        var currentCell = GetCurrentCell();
        var nextCell = GetNextCell(currentCell, Direction);

        System.Diagnostics.Debug.WriteLine($"[TRAIN] GetNextSignal - Current: {currentCell}, Next: {nextCell}, Dir: {Direction}");

        // Sprawdź aktualną komórkę
        var currentSignals = _signalController.GetSignalsAt(currentCell);
        var signal = currentSignals?.FirstOrDefault(s => s.Direction == Direction);

        if (signal != null)
        {
            System.Diagnostics.Debug.WriteLine($"[TRAIN] ✅ Found signal at CURRENT cell: {signal.Aspect} ({signal.GetAspectName()})");
            return signal;
        }

        // Sprawdź następną komórkę
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
        if (signal == null)
            return _maxSpeed;

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
    // VEHICLE TRAJECTORY
    // ============================================================
    private Vector2 GetPositionBehindHead(
        float distanceBehind)
    {
        if (distanceBehind <=
            MovementEpsilon)
        {
            return Position;
        }

        float targetDistance =
            _totalTravelDistance -
            distanceBehind;

        if (
            _trajectory.Count == 0 ||
            targetDistance <= 0.0f)
        {
            return Position -
                   DirectionToVector(Direction) *
                   distanceBehind;
        }

        for (
            int i = _trajectory.Count - 1;
            i > 0;
            i--)
        {
            TrajectoryPoint newer =
                _trajectory[i];

            TrajectoryPoint older =
                _trajectory[i - 1];

            if (
                targetDistance >=
                    older.Distance &&
                targetDistance <=
                    newer.Distance)
            {
                float span =
                    newer.Distance -
                    older.Distance;

                if (span <= MovementEpsilon)
                    return older.Position;

                float t =
                    (targetDistance -
                     older.Distance) /
                    span;

                return Vector2.Lerp(
                    older.Position,
                    newer.Position,
                    t);
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

        _curveCell =
            new MapPosition(0, 0);

        _curveEntrySide =
            TrackConnections.None;

        _curveExitSide =
            TrackConnections.None;

        _arcCenter =
            Vector2.Zero;

        _arcStartAngle = 0.0f;

        _arcSweepAngle = 0.0f;

        _curveDistance = 0.0f;

        _curveLength = 0.0f;
    }

    // ============================================================
    // DIRECTION HELPERS
    // ============================================================

    private static Vector2 DirectionToVector(
        TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North =>
                new Vector2(
                    0.0f,
                    -1.0f),

            TrackConnections.East =>
                new Vector2(
                    1.0f,
                    0.0f),

            TrackConnections.South =>
                new Vector2(
                    0.0f,
                    1.0f),

            TrackConnections.West =>
                new Vector2(
                    -1.0f,
                    0.0f),

            _ =>
                throw new ArgumentException(
                    "Direction must contain exactly one cardinal direction.",
                    nameof(direction))
        };
    }

    private static TrackConnections GetOppositeDirection(
        TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North =>
                TrackConnections.South,

            TrackConnections.East =>
                TrackConnections.West,

            TrackConnections.South =>
                TrackConnections.North,

            TrackConnections.West =>
                TrackConnections.East,

            _ =>
                TrackConnections.None
        };
    }

    private static void ValidateDirection(
        TrackConnections direction)
    {
        if (
            direction != TrackConnections.North &&
            direction != TrackConnections.East &&
            direction != TrackConnections.South &&
            direction != TrackConnections.West)
        {
            throw new ArgumentException(
                "Train direction must be a single cardinal direction.",
                nameof(direction));
        }
    }
}

// ============================================================
// MATH HELPER
// ============================================================

public static class MathHelper
{
    public const float PiOver2 =
        MathF.PI / 2.0f;

    public const float Pi =
        MathF.PI;

    public static float Clamp(
        float value,
        float min,
        float max)
    {
        return value < min
            ? min
            : value > max
                ? max
                : value;
    }

    public static float LerpAngle(
        float from,
        float to,
        float t)
    {
        float difference =
            MathF.IEEERemainder(
                to - from,
                MathF.PI * 2f);

        return from +
               difference * t;
    }
}
