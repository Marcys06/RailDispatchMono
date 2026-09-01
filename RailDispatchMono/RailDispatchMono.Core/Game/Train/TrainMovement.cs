using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using System;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    public void Update(float deltaTime)
    {
        if (deltaTime <= 0.0f || !CanMove || _map is null)
            return;

        DebugManager.Log(
            $"[TRAIN] ?? START - Pos: ({Position.X:F4}, {Position.Y:F4}), " +
            $"Dir: {Direction}, Speed: {Speed:F2} m/s ({Speed * 3.6f:F1} km/h)");

        var nextSignal = GetNextSignal();
        if (nextSignal != null)
        {
            _lastSignal = nextSignal;
            _lastSignalSpeed = GetSpeedFromSignal(nextSignal);
        }

        _targetSpeed = _lastSignalSpeed;

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

        float decelerationRate = maxBraking > 0 ? maxBraking : 20.0f;

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

        // Speed is stored in physical m/s. World movement is measured in grid cells,
        // so convert metres to cells at the single authoritative spatial boundary.
        float distance = SimulationScale.MetersToGrid(Speed * deltaTime);
        if (distance > MovementEpsilon)
            Move(distance);
    }

    private void Move(float distance)
    {
        if (_map is null) return;

        if (Position.X < 0 || Position.X > _map.Size.Width ||
            Position.Y < 0 || Position.Y > _map.Size.Height)
        {
            DebugManager.Log($"[TRAIN] WARNING: Train out of bounds! {Position}");
            _speed = 0;
            return;
        }

        float remaining = distance;
        int iterations = 0;
        MapPosition lastCell = GetCurrentCell();
        int sameCellCount = 0;

        while (remaining > MovementEpsilon)
        {
            if (++iterations > MaxMovementIterations)
            {
                DebugManager.Log("[TRAIN] Movement iteration limit reached.");
                break;
            }

            MapPosition currentCell = GetCurrentCell();

            if (currentCell == lastCell)
            {
                sameCellCount++;
                if (sameCellCount > 10)
                {
                    DebugManager.Log($"[TRAIN] Stuck in cell {currentCell} - forcing exit");
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

            if (!_map.TryGetTrack(currentCell, out TrackCell? track) || track is null)
            {
                DebugManager.Log($"[TRAIN] No track at {currentCell} - stopping");
                _speed = 0;
                break;
            }

            TrackConnections entrySide = GetOppositeDirection(Direction);

            if (track.Geometry == TrackGeometry.Junction)
            {
                if (!HandleJunction(track, currentCell, entrySide, ref remaining))
                    break;
                continue;
            }

            if (track.Geometry == TrackGeometry.Curve)
            {
                TrackConnections exitSide = GetCurveExitDirection(track.Connections, entrySide);
                bool isTurning = exitSide != TrackConnections.None && IsPerpendicular(entrySide, exitSide);

                if (isTurning)
                {
                    if (!EnterCurve(track))
                        break;
                    continue;
                }
                else
                {
                    DebugManager.Log($"[CURVE] Invalid entry {entrySide} at {currentCell} - stopping");
                    _speed = 0;
                    break;
                }
            }

            if (!HandleStraight(currentCell, ref remaining))
                break;
        }
    }

    private bool HandleStraight(MapPosition currentCell, ref float remaining)
    {
        if (_map is null) return false;
        if (!_map.TryGetTrack(currentCell, out TrackCell? track) || track is null)
            return false;
        if (!track.HasConnection(Direction))
        {
            DebugManager.Log($"[STRAIGHT] No connection {Direction} at {currentCell} - stopping");
            _speed = 0;
            return false;
        }

        float distanceToBoundary = GetDistanceToBoundary();
        if (distanceToBoundary <= MovementEpsilon)
            return EnterNextCell();

        float step = MathF.Min(remaining, distanceToBoundary);
        if (step < MovementEpsilon)
            step = MovementEpsilon;
        if (step > distanceToBoundary + MovementEpsilon)
            step = distanceToBoundary;

        MoveStraight(step);
        remaining -= step;

        float newDistanceToBoundary = GetDistanceToBoundary();
        if (newDistanceToBoundary <= MovementEpsilon)
        {
            if (!EnterNextCell())
            {
                DebugManager.Log("[STRAIGHT] Cannot enter next cell after boundary - stopping");
                _speed = 0;
                return false;
            }
        }

        return true;
    }

    private void MoveStraight(float distance)
    {
        if (distance <= 0.0f) return;

        Vector2 oldPos = Position;
        Vector2 movement = DirectionToVector(Direction) * distance;
        Vector2 newPos = oldPos + movement;

        MapPosition oldCell = GetCurrentCellFromPosition(oldPos);
        MapPosition newCell = GetCurrentCellFromPosition(newPos);

        if (newCell != oldCell && newCell != GetNextCell(oldCell, Direction))
        {
            DebugManager.Log(
                $"[STRAIGHT] WARNING: Moving through multiple cells! {oldCell} -> {newCell}");
            newPos = GetPositionAtEntry(newCell, Direction);
        }

        Position = newPos;
        DebugManager.Log(
            $"[STRAIGHT] Dir:{Direction} Dist:{distance:F6} " +
            $"Old:({oldPos.X:F4},{oldPos.Y:F4}) New:({Position.X:F4},{Position.Y:F4})");

        TotalDistance += distance;
        DistanceAlongTrack += distance;
        AddTrajectoryPoint(Position, distance);
    }

    private bool HandleJunction(TrackCell track, MapPosition currentCell, TrackConnections entrySide, ref float remaining)
    {
        DebugManager.Log($"[JUNCTION] Entering {currentCell}, Dir: {Direction}, Entry: {entrySide}");
        TrackConnections exitSide = track.GetExitDirection(entrySide);
        if (exitSide == TrackConnections.None)
        {
            DebugManager.Log($"[JUNCTION] No exit from {entrySide} - stopping");
            _speed = 0;
            return false;
        }

        DebugManager.Log($"[JUNCTION] Exit: {exitSide}, Switch: {track.CurrentSwitchPosition}");
        bool isTurning = IsPerpendicular(entrySide, exitSide);

        if (isTurning)
        {
            DebugManager.Log($"[JUNCTION] Turning {entrySide} -> {exitSide} - entering curve");
            if (!EnterCurve(track, entrySide, exitSide))
            {
                DebugManager.Log("[JUNCTION] Failed to enter curve - stopping");
                _speed = 0;
                return false;
            }
            return true;
        }

        Direction = exitSide;
        DebugManager.Log($"[JUNCTION] Going straight, new direction: {Direction}");
        Vector2 exitPos = GetPositionAtEntry(currentCell, exitSide);
        float transitionDist = Vector2.Distance(Position, exitPos);

        if (transitionDist > MovementEpsilon)
        {
            AddTrajectoryPoint(exitPos, transitionDist);
            TotalDistance += transitionDist;
            DistanceAlongTrack += transitionDist;
        }

        Position = exitPos;
        remaining -= transitionDist;
        return EnterNextCell();
    }

    private bool EnterCurve(TrackCell track)
    {
        TrackConnections entrySide = GetOppositeDirection(Direction);
        if (!track.HasConnection(entrySide))
            return false;

        TrackConnections exitSide = track.Geometry == TrackGeometry.Junction
            ? track.GetExitDirection(entrySide)
            : GetCurveExitDirection(track.Connections, entrySide);

        if (exitSide == TrackConnections.None || !IsPerpendicular(entrySide, exitSide))
            return false;

        return EnterCurve(track, entrySide, exitSide);
    }

    private bool EnterCurve(TrackCell track, TrackConnections entrySide, TrackConnections exitSide)
    {
        MapPosition cell = track.Position;
        Direction = exitSide;
        _curveCell = cell;
        _curveEntrySide = entrySide;
        _curveExitSide = exitSide;
        SetupArcParams(cell, entrySide, exitSide);
        _curveLength = DefaultCurveLength;
        _curveDistance = 0.0f;
        _isOnCurve = true;

        DebugManager.Log(
            $"[CURVE] Enter cell:{cell} Entry:{entrySide} Exit:{exitSide} " +
            $"Center:{_arcCenter} Start:{_arcStartAngle:F4} Sweep:{_arcSweepAngle:F4} Length:{_curveLength:F4}");
        return true;
    }

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

        _curveDistance += step;
        remaining -= step;
        float progress = MathHelper.Clamp(_curveDistance / _curveLength, 0.0f, 1.0f);
        Position = GetArcPosition(progress);
        TotalDistance += step;
        DistanceAlongTrack += step;
        AddTrajectoryPoint(Position, step);

        if (_curveDistance >= _curveLength - MovementEpsilon)
        {
            _curveDistance = _curveLength;
            Position = GetArcPosition(1.0f);
            FinishCurve();
        }
    }

    private void FinishCurve()
    {
        if (!_isOnCurve) return;

        MapPosition savedCurveCell = _curveCell;
        TrackConnections savedExitSide = _curveExitSide;
        float savedCurveDistance = _curveDistance;
        Position = GetArcPosition(1.0f);
        Direction = _curveExitSide;

        DebugManager.Log(
            $"[FINISH CURVE] Cell:{savedCurveCell} Position:{Position} " +
            $"Exit:{savedExitSide} CurveDistance:{savedCurveDistance:F6}");

        ResetCurveState();
        if (_map is null) return;

        MapPosition nextCell = GetNextCell(savedCurveCell, savedExitSide);
        if (_map.TryGetTrack(nextCell, out TrackCell? nextTrack) && nextTrack != null)
        {
            TrackConnections entrySide = GetOppositeDirection(savedExitSide);
            if (nextTrack.HasConnection(entrySide))
            {
                Vector2 oldPos = Position;
                Vector2 entryPos = GetPositionAtEntry(nextCell, savedExitSide);
                float transitionDistance = Vector2.Distance(oldPos, entryPos);
                if (transitionDistance > MovementEpsilon)
                {
                    AddTrajectoryPoint(entryPos, transitionDistance);
                    TotalDistance += transitionDistance;
                    DistanceAlongTrack += transitionDistance;
                }
                Position = entryPos;
                DebugManager.Log(
                    $"[FINISH CURVE] Entered {nextCell}, " +
                    $"curve distance: {savedCurveDistance:F6}, " +
                    $"transition: {transitionDistance:F6}");
            }
        }
    }

    private bool EnterNextCell()
    {
        if (_map is null) return false;
        MapPosition currentCell = GetCurrentCell();
        MapPosition nextCell = GetNextCell(currentCell, Direction);
        DebugManager.Log($"[ENTER] Current:{currentCell} Next:{nextCell} Dir:{Direction} Pos:{Position}");
        if (currentCell == nextCell) return false;
        if (!_map.TryGetTrack(nextCell, out TrackCell? nextTrack) || nextTrack is null)
        {
            DebugManager.Log($"[ENTER] No track at {nextCell}");
            return false;
        }

        TrackConnections entrySide = GetOppositeDirection(Direction);
        if (!nextTrack.HasConnection(entrySide))
        {
            DebugManager.Log($"[ENTER] No connection {entrySide} at {nextCell}");
            return false;
        }

        TrackConnections exitSide = nextTrack.GetExitDirection(entrySide);
        if (exitSide == TrackConnections.None)
        {
            DebugManager.Log($"[ENTER] No exit path available from {entrySide} at {nextCell}");
            return false;
        }

        if (IsPerpendicular(entrySide, exitSide))
            return EnterCurve(nextTrack, entrySide, exitSide);

        Direction = exitSide;
        Vector2 entryPos = GetPositionAtEntry(nextCell, Direction);
        Vector2 oldPos = Position;
        float actualDistance = Vector2.Distance(oldPos, entryPos);
        DebugManager.Log($"[ENTER] OldPos:{oldPos} NewPos:{entryPos} Distance:{actualDistance:F6}");

        if (actualDistance > MovementEpsilon)
        {
            AddTrajectoryPoint(entryPos, actualDistance);
            TotalDistance += actualDistance;
            DistanceAlongTrack += actualDistance;
        }

        Position = entryPos;
        return true;
    }

    private MapPosition GetCurrentCellFromPosition(Vector2 pos)
    {
        return new MapPosition((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y));
    }
}
