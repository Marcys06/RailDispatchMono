using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private const float MovementEpsilon = 0.0001f;
    private const float CurveRadius = 0.5f;
    private const float HalfPi = MathF.PI / 2f;
    private const float DefaultCurveLength = MathF.PI * CurveRadius / 2f;

    private bool _isOnCurve;
    private MapPosition _curveCell;
    private TrackConnections _curveEntrySide;
    private TrackConnections _curveExitSide;
    private Vector2 _arcCenter;
    private float _arcStartAngle;
    private float _arcSweepAngle;
    private float _curveDistance;
    private float _curveLength;

    private readonly List<TrajectoryPoint> _trajectory = new();
    private float _totalTravelDistance;

    private readonly record struct TrajectoryPoint(Vector2 Position, float Distance);

    private void ResetCurveState()
    {
        _isOnCurve = false;
        _curveCell = default;
        _curveEntrySide = TrackConnections.None;
        _curveExitSide = TrackConnections.None;
        _arcCenter = default;
        _arcStartAngle = 0f;
        _arcSweepAngle = 0f;
        _curveDistance = 0f;
        _curveLength = 0f;
    }

    private void ResetTrajectory()
    {
        _trajectory.Clear();
        _trajectory.Add(new TrajectoryPoint(Position, 0f));
        _totalTravelDistance = 0f;
    }

    private void AddTrajectoryPoint(Vector2 position, float distance)
    {
        if (_trajectory.Count > 0 && Vector2.DistanceSquared(_trajectory[^1].Position, position) <= MovementEpsilon * MovementEpsilon)
            return;
        _trajectory.Add(new TrajectoryPoint(position, distance));
    }

    private void StartCurve(MapPosition cell, TrackConnections entrySide, TrackConnections exitSide)
    {
        _isOnCurve = true;
        _curveCell = cell;
        _curveEntrySide = entrySide;
        _curveExitSide = exitSide;
        _curveDistance = 0f;
        _curveLength = DefaultCurveLength;
        ConfigureArc(cell, entrySide, exitSide);
    }

    private void ConfigureArc(MapPosition cell, TrackConnections entrySide, TrackConnections exitSide)
    {
        float x = cell.X;
        float y = cell.Y;
        if (entrySide == TrackConnections.East && exitSide == TrackConnections.North)
        {
            _arcCenter = new Vector2(x + 1.0f, y);
            _arcStartAngle = MathF.PI;
            _arcSweepAngle = -HalfPi;
            return;
        }
        if (entrySide == TrackConnections.East && exitSide == TrackConnections.South)
        {
            _arcCenter = new Vector2(x + 1.0f, y + 1.0f);
            _arcStartAngle = MathF.PI;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.West && exitSide == TrackConnections.North)
        {
            _arcCenter = new Vector2(x, y);
            _arcStartAngle = 0.0f;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.West && exitSide == TrackConnections.South)
        {
            _arcCenter = new Vector2(x, y + 1.0f);
            _arcStartAngle = 0.0f;
            _arcSweepAngle = -HalfPi;
            return;
        }
        if (entrySide == TrackConnections.North && exitSide == TrackConnections.East)
        {
            _arcCenter = new Vector2(x + 1.0f, y);
            _arcStartAngle = HalfPi;
            _arcSweepAngle = -HalfPi;
            return;
        }
        if (entrySide == TrackConnections.North && exitSide == TrackConnections.West)
        {
            _arcCenter = new Vector2(x, y);
            _arcStartAngle = HalfPi;
            _arcSweepAngle = HalfPi;
            return;
        }
        if (entrySide == TrackConnections.South && exitSide == TrackConnections.East)
        {
            _arcCenter = new Vector2(x + 1.0f, y + 1.0f);
            _arcStartAngle = MathF.PI;
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
        TrackConnections.East => Vector2.UnitX,
        TrackConnections.West => -Vector2.UnitX,
        TrackConnections.South => Vector2.UnitY,
        TrackConnections.North => -Vector2.UnitY,
        _ => Vector2.Zero
    };

    private static float GetDirectionAngle(TrackConnections direction) => direction switch
    {
        TrackConnections.East => 0f,
        TrackConnections.South => MathF.PI / 2f,
        TrackConnections.West => MathF.PI,
        TrackConnections.North => -MathF.PI / 2f,
        _ => 0f
    };

    private static TrackConnections VectorToDirection(float rotation)
    {
        Vector2 direction = new Vector2(MathF.Cos(rotation), MathF.Sin(rotation));
        if (MathF.Abs(direction.X) >= MathF.Abs(direction.Y))
            return direction.X >= 0f ? TrackConnections.East : TrackConnections.West;
        return direction.Y >= 0f ? TrackConnections.South : TrackConnections.North;
    }

    private static void ValidateDirection(TrackConnections direction)
    {
        if (direction is not (TrackConnections.North or TrackConnections.South or TrackConnections.East or TrackConnections.West))
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Train direction must be a cardinal track connection.");
    }
}
