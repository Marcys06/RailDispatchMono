using System;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public Vector2 Position { get; private set; }

    public float DistanceAlongTrack
    {
        get => Position.X;

        set
        {
            Position =
                new Vector2(
                    value,
                    Position.Y);
        }
    }

    public float Speed { get; set; }

    public TrackConnections Direction { get; private set; }

    private GameMap? _map;

    public Train()
    {
        Id =
            Guid.NewGuid();

        Composition =
            new TrainComposition();

        Position =
            new Vector2(
                14.5f,
                2.5f);

        Speed =
            0.4f;

        Direction =
            TrackConnections.West;
    }

    public bool CanMove =>
        Composition.CanMove;

    public float Length =>
        Composition.Length;

    public void SetMap(
        GameMap map)
    {
        _map = map;
    }

    public void Update(
        float deltaTime)
    {
        if (!CanMove ||
            _map is null ||
            Speed <= 0f)
        {
            return;
        }

        System.Diagnostics.Debug.WriteLine(
            $"TRAIN Position={Position} Direction={Direction}");

        Move(
            Speed * deltaTime);
    }

    private void Move(
        float distance)
    {
        if (_map is null)
            return;

        var remaining = distance;

        while (remaining > 0.00001f)
        {
            var currentCell =
                GetCurrentCell();

            if (!_map.TryGetTrack(
                    currentCell,
                    out var track) ||
                track is null)
            {
                return;
            }

            if (!track.HasConnection(
                    Direction))
            {
                return;
            }

            /*
             * ZAKRĘT
             */
            if (track.Geometry ==
                TrackGeometry.Curve)
            {
                if (!MoveThroughCurve(
                        track,
                        ref remaining))
                {
                    return;
                }

                continue;
            }

            /*
             * PROSTY TOR
             */
            var distanceToBoundary =
                GetDistanceToBoundary();

            if (distanceToBoundary <= 0.00001f)
            {
                if (!EnterNextCell())
                    return;

                continue;
            }

            var step =
                MathF.Min(
                    remaining,
                    distanceToBoundary);

            MoveStraight(step);

            remaining -= step;

            if (remaining <= 0.00001f)
                return;

            if (distanceToBoundary - step <=
                0.00001f)
            {
                if (!EnterNextCell())
                    return;
            }
        }
    }

    private bool MoveThroughCurve(
        TrackCell track,
        ref float remaining)
    {
        var center =
            new Vector2(
                track.Position.X + 0.5f,
                track.Position.Y + 0.5f);

        var distanceToCenter =
            Direction switch
            {
                TrackConnections.North =>
                    Position.Y - center.Y,

                TrackConnections.East =>
                    center.X - Position.X,

                TrackConnections.South =>
                    center.Y - Position.Y,

                TrackConnections.West =>
                    Position.X - center.X,

                _ =>
                    0f
            };

        distanceToCenter =
            MathF.Max(
                0f,
                distanceToCenter);

        /*
         * Najpierw dojeżdżamy do środka
         * komórki zakrętu.
         */
        if (distanceToCenter > 0.00001f)
        {
            var step =
                MathF.Min(
                    remaining,
                    distanceToCenter);

            MoveStraight(step);

            remaining -= step;

            if (remaining <= 0.00001f)
                return true;
        }

        /*
         * Ustawiamy dokładnie środek.
         */
        Position =
            center;

        /*
         * Direction to kierunek jazdy. Strona, którą pociąg
         * faktycznie wjechał do tej komórki (i która jest
         * obecna w track.Connections), to jej przeciwność.
         */
        var entrySide =
            GetOppositeDirection(
                Direction);

        var nextDirection =
            GetCurveExitDirection(
                track.Connections,
                entrySide);

        if (nextDirection ==
            TrackConnections.None)
        {
            return false;
        }

        Direction =
            nextDirection;

        return true;
    }

    private MapPosition GetCurrentCell()
    {
        return new MapPosition(
            (int)MathF.Floor(Position.X),
            (int)MathF.Floor(Position.Y));
    }

    private float GetDistanceToBoundary()
    {
        return Direction switch
        {
            TrackConnections.East =>
                MathF.Floor(Position.X) +
                1f -
                Position.X,

            TrackConnections.West =>
                Position.X -
                MathF.Floor(Position.X),

            TrackConnections.South =>
                MathF.Floor(Position.Y) +
                1f -
                Position.Y,

            TrackConnections.North =>
                Position.Y -
                MathF.Floor(Position.Y),

            _ =>
                0f
        };
    }

    private void MoveStraight(
        float distance)
    {
        switch (Direction)
        {
            case TrackConnections.East:

                Position +=
                    new Vector2(
                        distance,
                        0f);

                break;

            case TrackConnections.West:

                Position +=
                    new Vector2(
                        -distance,
                        0f);

                break;

            case TrackConnections.South:

                Position +=
                    new Vector2(
                        0f,
                        distance);

                break;

            case TrackConnections.North:

                Position +=
                    new Vector2(
                        0f,
                        -distance);

                break;
        }
    }

    private bool EnterNextCell()
    {
        if (_map is null)
            return false;

        var currentCell =
            GetCurrentCell();

        var nextCell =
            GetNextCell(
                currentCell);

        System.Diagnostics.Debug.WriteLine(
            $"TRAIN EnterNextCell: " +
            $"current=({currentCell.X},{currentCell.Y}) " +
            $"next=({nextCell.X},{nextCell.Y}) " +
            $"direction={Direction}");

        if (!_map.TryGetTrack(
                nextCell,
                out var nextTrack) ||
            nextTrack is null)
        {
            System.Diagnostics.Debug.WriteLine(
                $"TRAIN BLOCKED: " +
                $"no track at ({nextCell.X},{nextCell.Y})");

            return false;
        }

        System.Diagnostics.Debug.WriteLine(
            $"TRAIN NextTrack: " +
            $"Geometry={nextTrack.Geometry} " +
            $"Connections={nextTrack.Connections}");

        /*
         * Direction oznacza kierunek jazdy.
         *
         * Do następnej komórki wchodzimy od strony
         * przeciwnej do kierunku jazdy.
         *
         * East -> wejście od West
         * West -> wejście od East
         * South -> wejście od North
         * North -> wejście od South
         */
        var direction =
            Direction;

        var entrySide =
            GetOppositeDirection(
                direction);

        /*
         * Sprawdzamy rzeczywistą stronę wejścia
         * do następnej komórki.
         */
        if (!nextTrack.HasConnection(
                entrySide))
        {
            System.Diagnostics.Debug.WriteLine(
                $"TRAIN BLOCKED: " +
                $"next track does not have entry connection " +
                $"{entrySide}");

            return false;
        }

        /*
         * Kierunek jazdy pozostaje bez zmian.
         *
         * Jeżeli nextTrack jest zakrętem,
         * MoveThroughCurve() zmieni Direction
         * dopiero po dotarciu do środka komórki.
         */
        Direction =
            direction;

        Position =
            GetPositionAtEntry(
                nextCell,
                direction);

        System.Diagnostics.Debug.WriteLine(
            $"TRAIN ENTERED: " +
            $"Position={Position} " +
            $"Direction={Direction} " +
            $"EntrySide={entrySide}");

        return true;
    }

    private MapPosition GetNextCell(
        MapPosition currentCell)
    {
        return Direction switch
        {
            TrackConnections.North =>
                new MapPosition(
                    currentCell.X,
                    currentCell.Y - 1),

            TrackConnections.East =>
                new MapPosition(
                    currentCell.X + 1,
                    currentCell.Y),

            TrackConnections.South =>
                new MapPosition(
                    currentCell.X,
                    currentCell.Y + 1),

            TrackConnections.West =>
                new MapPosition(
                    currentCell.X - 1,
                    currentCell.Y),

            _ =>
                currentCell
        };
    }

    private static Vector2 GetPositionAtEntry(
        MapPosition cell,
        TrackConnections direction)
    {
        const float epsilon = 0.0001f;

        return direction switch
        {
            /*
             * Jedziemy na wschód.
             * Wchodzimy od lewej strony następnej komórki.
             */
            TrackConnections.East =>
                new Vector2(
                    cell.X + epsilon,
                    cell.Y + 0.5f),

            /*
             * Jedziemy na zachód.
             * Wchodzimy od prawej strony następnej komórki.
             */
            TrackConnections.West =>
                new Vector2(
                    cell.X + 1f - epsilon,
                    cell.Y + 0.5f),

            /*
             * Jedziemy na południe.
             * Wchodzimy od góry.
             */
            TrackConnections.South =>
                new Vector2(
                    cell.X + 0.5f,
                    cell.Y + epsilon),

            /*
             * Jedziemy na północ.
             * Wchodzimy od dołu.
             */
            TrackConnections.North =>
                new Vector2(
                    cell.X + 0.5f,
                    cell.Y + 1f - epsilon),

            _ =>
                new Vector2(
                    cell.X + 0.5f,
                    cell.Y + 0.5f)
        };
    }

    /// <summary>
    /// Zwraca stronę geometrycznie przeciwną do podanej.
    /// Używana do wyznaczania strony wejścia do komórki
    /// (przeciwność kierunku jazdy) oraz strony wyjścia
    /// z zakrętu (na podstawie strony wejścia).
    /// </summary>
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

    /// <summary>
    /// Wyznacza kierunek wyjazdu z komórki zakrętu.
    /// Zakręt ma zawsze dokładnie dwie flagi połączeń;
    /// wyjście to ta druga flaga, różna od strony wejścia.
    /// </summary>
    private static TrackConnections GetCurveExitDirection(
        TrackConnections connections,
        TrackConnections entrySide)
    {
        if (!connections.HasFlag(entrySide))
            return TrackConnections.None;

        return connections & ~entrySide;
    }

    public Vector2 GetHeadPosition()
    {
        return Position;
    }

    public void SetDirection(
        TrackConnections direction)
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

        Direction =
            direction;
    }

    public float GetVehicleDistance(
        int vehicleIndex)
    {
        if (vehicleIndex < 0 ||
            vehicleIndex >= Composition.Vehicles.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(vehicleIndex));
        }

        var distance =
            Length;

        for (var i = 0;
             i < vehicleIndex;
             i++)
        {
            distance -=
                Composition.Vehicles[i]
                    .Parameters.Length;
        }

        return Position.Length() -
               distance;
    }
}