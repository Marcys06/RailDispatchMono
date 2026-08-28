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
            2.0f;

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
         * W środku zakrętu zmieniamy kierunek.
         */
        var nextDirection =
            GetCurveExitDirection(
                track.Connections,
                Direction);

        if (nextDirection ==
            TrackConnections.None)
        {
            return false;
        }

        Console.WriteLine(
            $"CURVE: {track.Position} " +
            $"Connections={track.Connections} " +
            $"{Direction} -> {nextDirection}");

        Direction =
            nextDirection;

        return true;
    }

    

    private bool HandleCurve(
        TrackCell track,
        ref float remaining)
    {
        var center =
            new Vector2(
                track.Position.X + 0.5f,
                track.Position.Y + 0.5f);

        /*
         * Sprawdźmy, czy pociąg jest już
         * w środku zakrętu.
         */
        var dx =
            MathF.Abs(
                Position.X -
                center.X);

        var dy =
            MathF.Abs(
                Position.Y -
                center.Y);

        var atCenter =
            dx <= 0.0001f &&
            dy <= 0.0001f;

        /*
         * Najpierw dojeżdżamy do środka
         * po aktualnym kierunku.
         */
        if (!atCenter)
        {
            var distanceToCenter =
                GetDistanceToCenter(
                    center);

            var step =
                MathF.Min(
                    remaining,
                    distanceToCenter);

            MoveStraight(
                step);

            remaining -=
                step;

            if (remaining <=
                0.0001f)
            {
                return true;
            }

            /*
             * Ustawiamy dokładnie środek,
             * żeby uniknąć błędów float.
             */
            Position =
                center;
        }

        /*
         * Jesteśmy w środku zakrętu.
         *
         * Direction wskazuje stronę,
         * z której przyjechaliśmy.
         *
         * Wybieramy drugie połączenie.
         */
        var nextDirection =
            GetCurveExitDirection(
                track.Connections,
                Direction);

        if (nextDirection ==
            TrackConnections.None)
        {
            return false;
        }

        Direction =
            nextDirection;

        /*
         * Pozostały dystans zostanie
         * wykonany już w nowym kierunku.
         */
        return true;
    }

    private float GetDistanceToCenter(
        Vector2 center)
    {
        return Direction switch
        {
            TrackConnections.East =>
                MathF.Max(
                    0f,
                    center.X -
                    Position.X),

            TrackConnections.West =>
                MathF.Max(
                    0f,
                    Position.X -
                    center.X),

            TrackConnections.South =>
                MathF.Max(
                    0f,
                    center.Y -
                    Position.Y),

            TrackConnections.North =>
                MathF.Max(
                    0f,
                    Position.Y -
                    center.Y),

            _ =>
                0f
        };
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
         * Pociąg wchodzi do następnej komórki
         * dokładnie od strony, z której przyjechał.
         */
        var direction =
            Direction;

        if (!nextTrack.HasConnection(
                direction))
        {
            System.Diagnostics.Debug.WriteLine(
                $"TRAIN BLOCKED: " +
                $"next track does not have connection {direction}");

            return false;
        }

        /*
         * Zachowujemy kierunek wejścia.
         *
         * Jeżeli następna komórka jest zakrętem,
         * zmiana kierunku nastąpi dopiero
         * po dotarciu do jej środka.
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
            $"Direction={Direction}");

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

    private static TrackConnections GetCurveExitDirection(
    TrackConnections connections,
    TrackConnections direction)
    {
        if (connections ==
            (TrackConnections.North | TrackConnections.East))
        {
            return direction switch
            {
                TrackConnections.North =>
                    TrackConnections.East,

                TrackConnections.East =>
                    TrackConnections.North,

                _ =>
                    TrackConnections.None
            };
        }

        if (connections ==
            (TrackConnections.East | TrackConnections.South))
        {
            return direction switch
            {
                TrackConnections.East =>
                    TrackConnections.South,

                TrackConnections.South =>
                    TrackConnections.East,

                _ =>
                    TrackConnections.None
            };
        }

        if (connections ==
            (TrackConnections.South | TrackConnections.West))
        {
            return direction switch
            {
                TrackConnections.South =>
                    TrackConnections.West,

                TrackConnections.West =>
                    TrackConnections.South,

                _ =>
                    TrackConnections.None
            };
        }

        if (connections ==
            (TrackConnections.West | TrackConnections.North))
        {
            return direction switch
            {
                TrackConnections.West =>
                    TrackConnections.North,

                TrackConnections.North =>
                    TrackConnections.West,

                _ =>
                    TrackConnections.None
            };
        }

        return TrackConnections.None;
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