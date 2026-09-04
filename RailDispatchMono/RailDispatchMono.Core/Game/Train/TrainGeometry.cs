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

    private bool TryGetTrajectoryTransformBehindHead(float distanceBehind, out Vector2 position, out float rotation)
    {
        position = default;
        rotation = 0f;

        if (distanceBehind <= MovementEpsilon || _trajectory.Count < 2)
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
