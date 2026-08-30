using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    // ============================================================
    // CURVE STATE
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
    // MAP REFERENCE
    // ============================================================

    private GameMap? _map;

    // ============================================================
    // CURVE STATE MANAGEMENT
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
    // TRAJECTORY MANAGEMENT
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

    // ============================================================
    // ARC GEOMETRY
    // ============================================================

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

    // ============================================================
    // POSITION HELPERS
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

    private static TrackConnections GetCurveExitDirection(TrackConnections connections, TrackConnections entrySide)
    {
        if (!connections.HasFlag(entrySide))
            return TrackConnections.None;

        TrackConnections exits = connections & ~entrySide;

        if (exits == TrackConnections.None)
            return TrackConnections.None;

        if (exits.HasFlag(TrackConnections.North))
            return TrackConnections.North;
        if (exits.HasFlag(TrackConnections.East))
            return TrackConnections.East;
        if (exits.HasFlag(TrackConnections.South))
            return TrackConnections.South;
        if (exits.HasFlag(TrackConnections.West))
            return TrackConnections.West;

        return TrackConnections.None;
    }

    private static bool IsPerpendicular(TrackConnections first, TrackConnections second)
    {
        bool firstHorizontal = first == TrackConnections.East || first == TrackConnections.West;
        bool secondHorizontal = second == TrackConnections.East || second == TrackConnections.West;
        return firstHorizontal != secondHorizontal;
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

    private static float GetDirectionAngle(TrackConnections direction)
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

    private static TrackConnections VectorToDirection(float angle)
    {
        while (angle > MathF.PI)
            angle -= MathF.Tau;
        while (angle < -MathF.PI)
            angle += MathF.Tau;

        float absAngle = MathF.Abs(angle);

        if (absAngle < MathF.PI / 4f || absAngle > 3f * MathF.PI / 4f)
        {
            return angle >= 0f ? TrackConnections.East : TrackConnections.West;
        }

        return angle >= 0f ? TrackConnections.South : TrackConnections.North;
    }

    private static void ValidateDirection(TrackConnections direction)
    {
        if (direction != TrackConnections.North &&
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
    public const float PiOver2 = MathF.PI / 2.0f;
    public const float Pi = MathF.PI;

    public static float Clamp(float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }

    public static float LerpAngle(float from, float to, float t)
    {
        float difference = MathF.IEEERemainder(to - from, MathF.PI * 2f);
        return from + difference * t;
    }
}