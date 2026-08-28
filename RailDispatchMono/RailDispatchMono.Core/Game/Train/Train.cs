// ============================================================
// GRID MOVEMENT
// ============================================================

using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

/// <summary>
/// Przetwarza cały dystans ruchu.
/// Dystans jest dzielony na fragmenty prostej, granice komórek i łuki.
/// </summary>
private void Move(float distance)
{
    if (_map is null) return;

    // SANITY CHECK: Jeśli pociąg jest poza mapą, zatrzymaj
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

/// <summary>
/// Przechodzi do następnej komórki.
/// </summary>
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

/// <summary>
/// Przesuwa głowę po aktualnym łuku.
/// </summary>
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

    if (_curveLength - _curveDistance <= MovementEpsilon)
    {
        FinishCurve();
    }
}

// ============================================================
// ARC GEOMETRY
// ============================================================

/// <summary>
/// Wyznacza środek okręgu oraz kąty łuku.
/// </summary>
private void SetupArcParams(MapPosition cell, TrackConnections entrySide, TrackConnections exitSide)
{
    // WEST -> NORTH / NORTH -> WEST - Lewy górny narożnik
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

    // EAST -> NORTH / NORTH -> EAST - Prawy górny narożnik
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

    // EAST -> SOUTH / SOUTH -> EAST - Prawy dolny narożnik
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

    // WEST -> SOUTH / SOUTH -> WEST - Lewy dolny narożnik
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

/// <summary>
/// Zwraca pozycję na łuku dla progress 0..1.
/// </summary>
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

/// <summary>
/// Zwraca komórkę, w której znajduje się głowa.
/// </summary>
public MapPosition GetCurrentCell()
{
    return new MapPosition(
        (int)MathF.Floor(Position.X),
        (int)MathF.Floor(Position.Y));
}

/// <summary>
/// Dystans do granicy aktualnego kafelka w kierunku jazdy.
/// </summary>
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

/// <summary>
/// Pozycja minimalnie wewnątrz komórki po przekroczeniu jej krawędzi.
/// </summary>
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

/// <summary>
/// Zwraca obrót głowy pociągu w radianach.
/// </summary>
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

/// <summary>
/// Zwraca pozycje kolejnych pojazdów składu.
/// </summary>
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

/// <summary>
/// Zwraca dystans logicznego punktu pojazdu od głowy.
/// </summary>
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

// ============================================================
// MATH HELPER (jeśli nie masz w projekcie)
// ============================================================

public static class MathHelper
{
    public const float PiOver2 = MathF.PI / 2.0f;
    public const float Pi = MathF.PI;

    public static float Clamp(float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }
}