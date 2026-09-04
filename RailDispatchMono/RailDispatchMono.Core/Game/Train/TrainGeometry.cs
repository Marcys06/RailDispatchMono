using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private bool _isOnCurve;
    private MapPosition _curveCell;
    private TrackConnections _curveEntrySide;
    private TrackConnections _curveExitSide;
    private Vector2 _arcCenter;
    private float _arcStartAngle;
    private float _arcSweepAngle;
    private float _curveDistance;
    private float _curveLength;

    private const float CurveRadius = 0.5f;
    private const float HalfPi = MathF.PI * 0.5f;
    private const float DefaultCurveLength = MathF.PI * CurveRadius * 0.5f;
    private const float MovementEpsilon = 0.00001f;
    private const int MaxMovementIterations = 256;

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

    private GameMap? _map;

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

    private void ResetTrajectory()
    {
        _trajectory.Clear();
        _totalTravelDistance = 0f;
        _trajectory.Add(new TrajectoryPoint(Position, 0f));
    }

    private void AddTrajectoryPoint(Vector2 position, float travelledDistance)
    {
        if (travelledDistance > 0.0f)
            _totalTravelDistance += travelledDistance;

        if (_trajectory.Count > 0)
        {
            TrajectoryPoint last = _trajectory[_trajectory.Count - 1];
            if ((last.Position - position).LengthSquared() < MovementEpsilon * MovementEpsilon)
                return;
        }

        _trajectory.Add(new TrajectoryPoint(position, _totalTravelDistance));

        float requiredHistory = MathF.Max(Length * 25.0f, 60.0f);
        float minimumDistance = _totalTravelDistance - requiredHistory;

        while (_trajectory.Count > 2 && _trajectory[1].Distance < minimumDistance)
            _trajectory.RemoveAt(0);
    }

    private void SetupArcParams(MapPosition cell, TrackConnections entrySide, TrackConnections exitSide)
    {
        float x = cell.X;
        float y = cell.Y;

        if (entrySide == TrackConnections.West && exitSide == TrackConnections.North)
        {
            _arcCenter = new Vector2(x, y);
            _arcStartAngle = 0.0f;
            _arcSweepAngle = -HalfPi;
            return;
        }
        if (entrySide == TrackConnections.North && exitSide == TrackConnections.West)
        {
            _arcCenter = new Vector2(x, y);
            _arcStartAngle = 0.0f;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.East && exitSide == TrackConnections.North)
        {
            _arcCenter = new Vector2(x + 1.0f, y);
            _arcStartAngle = HalfPi;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.North && exitSide == TrackConnections.East)
        {
            _arcCenter = new Vector2(x + 1.0f, y);
            _arcStartAngle = MathF.PI;
            _arcSweepAngle = -HalfPi;
            return;
        }
        if (entrySide == TrackConnections.East && exitSide == TrackConnections.South)
        {
            _arcCenter = new Vector2(x + 1.0f, y + 1.0f);
            _arcStartAngle = -HalfPi;
            _arcSweepAngle = -HalfPi;
            return;
        }
        if (entrySide == TrackConnections.South && exitSide == TrackConnections.East)
        {
            _arcCenter = new Vector2(x + 1.0f, y + 1.0f);
            _arcStartAngle = MathF.PI;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.West && exitSide == TrackConnections.South)
        {
            _arcCenter = new Vector2(x, y + 1.0f);
            _arcStartAngle = -HalfPi;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.South && exitSide == TrackConnections.West)
        {
            _arcCenter = new Vector2(x, y + 1.0f);
            _arcStartAngle = 0.0f;
            _arcSweepAngle = -HalfPi;
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

    private Vector2 GetPositionBehindHead(float distanceBehind)
    {
        if (distanceBehind <= MovementEpsilon)
            return Position;

        float targetDistance = _totalTravelDistance - distanceBehind;

        if (_trajectory.Count == 0 || targetDistance <= 0.0f)
            return Position - DirectionToVector(Direction) * distanceBehind;

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

    private bool TryGetTrajectoryTransformBehindHead(float distanceBehind, out Vector2 position, out float rotation)
    {
        position = default;
        rotation = 0f;

        if (distanceBehind <= MovementEpsilon)
            return false;

        if (!_isReversed)
            return TryGetForwardTrackTransform(distanceBehind, out position, out rotation);

        if (_trajectory.Count < 2)
            return false;

        float targetDistance = _totalTravelDistance - distanceBehind;
        if (targetDistance < _trajectory[0].Distance - MovementEpsilon)
            return false;

        int upperIndex = -1;
        for (int i = 1; i < _trajectory.Count; i++)
        {
            if (_trajectory[i].Distance >= targetDistance)
            {
                upperIndex = i;
                break;
            }
        }

        if (upperIndex < 0)
            upperIndex = _trajectory.Count - 1;

        int lowerIndex = Math.Max(0, upperIndex - 1);
        TrajectoryPoint lower = _trajectory[lowerIndex];
        TrajectoryPoint upper = _trajectory[upperIndex];

        float span = upper.Distance - lower.Distance;
        if (span > MovementEpsilon)
        {
            float t = MathHelper.Clamp((targetDistance - lower.Distance) / span, 0f, 1f);
            position = Vector2.Lerp(lower.Position, upper.Position, t);
        }
        else
        {
            position = upper.Position;
        }

        Vector2 tangent;
        if (upperIndex < _trajectory.Count - 1)
            tangent = _trajectory[upperIndex + 1].Position - lower.Position;
        else
            tangent = upper.Position - lower.Position;

        if (tangent.LengthSquared() <= MovementEpsilon * MovementEpsilon)
            return false;

        tangent.Normalize();
        rotation = MathF.Atan2(tangent.Y, tangent.X);
        return true;
    }

    private bool TryGetForwardTrackTransform(float distanceAhead, out Vector2 position, out float rotation)
    {
        position = Position;
        rotation = GetRotation();

        if (_map is null || distanceAhead <= MovementEpsilon)
            return distanceAhead <= MovementEpsilon;

        Vector2 simulatedPosition = Position;
        TrackConnections simulatedDirection = Direction;
        MapPosition simulatedCell = GetCurrentCellFromPosition(simulatedPosition);

        bool onCurve = _isOnCurve;
        MapPosition curveCell = _curveCell;
        TrackConnections curveEntry = _curveEntrySide;
        TrackConnections curveExit = _curveExitSide;
        Vector2 arcCenter = _arcCenter;
        float arcStartAngle = _arcStartAngle;
        float arcSweepAngle = _arcSweepAngle;
        float curveDistance = _curveDistance;
        float curveLength = _curveLength;

        float remaining = distanceAhead;
        int iterations = 0;

        while (remaining > MovementEpsilon && ++iterations <= MaxMovementIterations)
        {
            if (onCurve)
            {
                float curveRemaining = curveLength - curveDistance;
                if (curveRemaining <= MovementEpsilon)
                {
                    simulatedPosition = arcCenter + new Vector2(
                        MathF.Cos(arcStartAngle + arcSweepAngle),
                        MathF.Sin(arcStartAngle + arcSweepAngle)) * CurveRadius;
                    simulatedDirection = curveExit;
                    simulatedCell = GetNextCell(curveCell, curveExit);
                    onCurve = false;
                    continue;
                }

                float step = MathF.Min(remaining, curveRemaining);
                curveDistance += step;
                remaining -= step;

                float progress = MathHelper.Clamp(curveDistance / curveLength, 0f, 1f);
                float angle = arcStartAngle + arcSweepAngle * progress;
                simulatedPosition = arcCenter + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * CurveRadius;

                Vector2 tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
                if (arcSweepAngle < 0f)
                    tangent = -tangent;

                if (remaining <= MovementEpsilon)
                {
                    position = simulatedPosition;
                    rotation = MathF.Atan2(tangent.Y, tangent.X);
                    return true;
                }

                if (curveDistance >= curveLength - MovementEpsilon)
                {
                    simulatedPosition = arcCenter + new Vector2(
                        MathF.Cos(arcStartAngle + arcSweepAngle),
                        MathF.Sin(arcStartAngle + arcSweepAngle)) * CurveRadius;
                    simulatedDirection = curveExit;
                    simulatedCell = GetNextCell(curveCell, curveExit);
                    onCurve = false;
                }
                continue;
            }

            if (!_map.TryGetTrack(simulatedCell, out TrackCell? track) || track is null)
                return false;

            TrackConnections entrySide = simulatedDirection.GetOppositeDirection();
            TrackConnections exitSide;

            if (track.Geometry == TrackGeometry.Junction)
            {
                exitSide = track.GetExitDirection(entrySide);
                if (exitSide == TrackConnections.None)
                    return false;

                if (IsPerpendicular(entrySide, exitSide))
                {
                    curveCell = simulatedCell;
                    curveEntry = entrySide;
                    curveExit = exitSide;
                    SetupLocalArcParams(curveCell, curveEntry, curveExit,
                        out arcCenter, out arcStartAngle, out arcSweepAngle);
                    curveLength = DefaultCurveLength;
                    curveDistance = 0f;
                    onCurve = true;
                    continue;
                }

                simulatedDirection = exitSide;
                Vector2 exitPosition = GetPositionAtEntry(simulatedCell, exitSide);
                float transitionDistance = Vector2.Distance(simulatedPosition, exitPosition);
                if (transitionDistance > remaining)
                {
                    Vector2 delta = exitPosition - simulatedPosition;
                    if (delta.LengthSquared() <= MovementEpsilon * MovementEpsilon)
                        return false;
                    delta.Normalize();
                    position = simulatedPosition + delta * remaining;
                    rotation = MathF.Atan2(delta.Y, delta.X);
                    return true;
                }

                simulatedPosition = exitPosition;
                remaining -= transitionDistance;
                continue;
            }

            if (track.Geometry == TrackGeometry.Curve)
            {
                exitSide = GetCurveExitDirection(track.Connections, entrySide);
                if (exitSide == TrackConnections.None || !IsPerpendicular(entrySide, exitSide))
                    return false;

                curveCell = simulatedCell;
                curveEntry = entrySide;
                curveExit = exitSide;
                SetupLocalArcParams(curveCell, curveEntry, curveExit,
                    out arcCenter, out arcStartAngle, out arcSweepAngle);
                curveLength = DefaultCurveLength;
                curveDistance = 0f;
                onCurve = true;
                continue;
            }

            if (!track.HasConnection(simulatedDirection))
                return false;

            float distanceToBoundary = simulatedDirection switch
            {
                TrackConnections.East => simulatedCell.X + 1f - simulatedPosition.X,
                TrackConnections.West => simulatedPosition.X - simulatedCell.X,
                TrackConnections.South => simulatedCell.Y + 1f - simulatedPosition.Y,
                TrackConnections.North => simulatedPosition.Y - simulatedCell.Y,
                _ => 0f
            };

            distanceToBoundary = MathF.Max(0f, distanceToBoundary);
            float straightStep = MathF.Min(remaining, distanceToBoundary);
            if (straightStep > MovementEpsilon)
            {
                simulatedPosition += DirectionToVector(simulatedDirection) * straightStep;
                remaining -= straightStep;
            }

            if (remaining <= MovementEpsilon)
            {
                position = simulatedPosition;
                rotation = GetDirectionAngle(simulatedDirection);
                return true;
            }

            MapPosition nextCell = GetNextCell(simulatedCell, simulatedDirection);
            if (!_map.TryGetTrack(nextCell, out TrackCell? nextTrack) || nextTrack is null)
                return false;

            TrackConnections nextEntry = simulatedDirection.GetOppositeDirection();
            TrackConnections nextExit = nextTrack.GetExitDirection(nextEntry);
            if (nextExit == TrackConnections.None)
                return false;

            simulatedCell = nextCell;
            simulatedDirection = nextExit;

            if (IsPerpendicular(nextEntry, nextExit))
            {
                curveCell = simulatedCell;
                curveEntry = nextEntry;
                curveExit = nextExit;
                SetupLocalArcParams(curveCell, curveEntry, curveExit,
                    out arcCenter, out arcStartAngle, out arcSweepAngle);
                curveLength = DefaultCurveLength;
                curveDistance = 0f;
                onCurve = true;
            }
            else
            {
                simulatedPosition = GetPositionAtEntry(nextCell, nextExit);
            }
        }

        return false;
    }

    private static void SetupLocalArcParams(
        MapPosition cell,
        TrackConnections entrySide,
        TrackConnections exitSide,
        out Vector2 center,
        out float startAngle,
        out float sweepAngle)
    {
        float x = cell.X;
        float y = cell.Y;

        if (entrySide == TrackConnections.West && exitSide == TrackConnections.North)
        {
            center = new Vector2(x, y); startAngle = 0f; sweepAngle = -HalfPi; return;
        }
        if (entrySide == TrackConnections.North && exitSide == TrackConnections.West)
        {
            center = new Vector2(x, y); startAngle = 0f; sweepAngle = HalfPi; return;
        }
        if (entrySide == TrackConnections.East && exitSide == TrackConnections.North)
        {
            center = new Vector2(x + 1f, y); startAngle = HalfPi; sweepAngle = HalfPi; return;
        }
        if (entrySide == TrackConnections.North && exitSide == TrackConnections.East)
        {
            center = new Vector2(x + 1f, y); startAngle = MathF.PI; sweepAngle = -HalfPi; return;
        }
        if (entrySide == TrackConnections.East && exitSide == TrackConnections.South)
        {
            center = new Vector2(x + 1f, y + 1f); startAngle = -HalfPi; sweepAngle = -HalfPi; return;
        }
        if (entrySide == TrackConnections.South && exitSide == TrackConnections.East)
        {
            center = new Vector2(x + 1f, y + 1f); startAngle = MathF.PI; sweepAngle = HalfPi; return;
        }
        if (entrySide == TrackConnections.West && exitSide == TrackConnections.South)
        {
            center = new Vector2(x, y + 1f); startAngle = -HalfPi; sweepAngle = HalfPi; return;
        }
        if (entrySide == TrackConnections.South && exitSide == TrackConnections.West)
        {
            center = new Vector2(x, y + 1f); startAngle = 0f; sweepAngle = -HalfPi; return;
        }

        throw new InvalidOperationException($"Unsupported curve: {entrySide} -> {exitSide}");
    }

    private static Vector2 GetPositionAtEntry(MapPosition cell, TrackConnections direction)
    {
        const float epsilon = 0.0001f;
        return direction switch
        {
            TrackConnections.East => new Vector2(cell.X + epsilon, cell.Y + 0.5f),
            TrackConnections.West => new Vector2(cell.X + 1.0f - epsilon, cell.Y + 0.5f),
            TrackConnections.South => new Vector2(cell.X + 0.5f, cell.Y + epsilon),
            TrackConnections.North => new Vector2(cell.X + 0.5f, cell.Y + 1.0f - epsilon),
            _ => new Vector2(cell.X + 0.5f, cell.Y + 0.5f)
        };
    }

    private static Vector2 DirectionToVector(TrackConnections direction) => direction switch
    {
        TrackConnections.North => new Vector2(0.0f, -1.0f),
        TrackConnections.East => new Vector2(1.0f, 0.0f),
        TrackConnections.South => new Vector2(0.0f, 1.0f),
        TrackConnections.West => new Vector2(-1.0f, 0.0f),
        _ => throw new ArgumentException("Direction must contain exactly one cardinal direction.", nameof(direction))
    };

    private static TrackConnections GetCurveExitDirection(TrackConnections connections, TrackConnections entrySide)
    {
        if (!connections.HasFlag(entrySide)) return TrackConnections.None;
        TrackConnections exits = connections & ~entrySide;
        if (exits == TrackConnections.None) return TrackConnections.None;
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

    private static MapPosition GetNextCell(MapPosition cell, TrackConnections direction) => direction switch
    {
        TrackConnections.North => new MapPosition(cell.X, cell.Y - 1),
        TrackConnections.East => new MapPosition(cell.X + 1, cell.Y),
        TrackConnections.South => new MapPosition(cell.X, cell.Y + 1),
        TrackConnections.West => new MapPosition(cell.X - 1, cell.Y),
        _ => cell
    };

    private static float GetDirectionAngle(TrackConnections direction) => direction switch
    {
        TrackConnections.East => 0f,
        TrackConnections.South => MathHelper.PiOver2,
        TrackConnections.West => MathHelper.Pi,
        TrackConnections.North => -MathHelper.PiOver2,
        _ => 0f
    };

    private static TrackConnections VectorToDirection(float angle)
    {
        while (angle > MathF.PI) angle -= MathF.Tau;
        while (angle < -MathF.PI) angle += MathF.Tau;
        float absAngle = MathF.Abs(angle);
        if (absAngle < MathF.PI / 4f || absAngle > 3f * MathF.PI / 4f)
            return angle >= 0f ? TrackConnections.East : TrackConnections.West;
        return angle >= 0f ? TrackConnections.South : TrackConnections.North;
    }

    private static void ValidateDirection(TrackConnections direction)
    {
        if (direction != TrackConnections.North && direction != TrackConnections.East &&
            direction != TrackConnections.South && direction != TrackConnections.West)
            throw new ArgumentException("Train direction must be a single cardinal direction.", nameof(direction));
    }
}
