using System;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public Vector2 Position { get; set; }

    public float Speed { get; set; }

    private GameMap? _map;

    private TrackConnections _direction =
        TrackConnections.East;

    public Train()
    {
        Id =
            Guid.NewGuid();

        Composition =
            new TrainComposition();

        Position =
            new Vector2(
                2.5f,
                2.5f);

        Speed =
            2.0f;
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
            deltaTime <= 0f)
        {
            return;
        }

        var remainingDistance =
            Speed * deltaTime;

        while (remainingDistance > 0f)
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

            var outgoing =
                GetOutgoingConnection(
                    track);

            if (outgoing ==
                TrackConnections.None)
            {
                return;
            }

            if (outgoing != _direction)
            {
                _direction =
                    outgoing;
            }

            var targetCell =
                GetNeighbour(
                    currentCell,
                    outgoing);

            if (!_map.TryGetTrack(
                    targetCell,
                    out var targetTrack) ||
                targetTrack is null)
            {
                return;
            }

            var targetPosition =
                GetCellCenter(
                    targetCell);

            var direction =
                targetPosition -
                Position;

            var distance =
                direction.Length();

            if (distance <= 0.0001f)
            {
                Position =
                    targetPosition;

                _direction =
                    outgoing;

                continue;
            }

            direction.Normalize();

            var step =
                MathF.Min(
                    remainingDistance,
                    distance);

            Position +=
                direction *
                step;

            remainingDistance -=
                step;

            if (step >=
                distance - 0.0001f)
            {
                Position =
                    targetPosition;

                _direction =
                    outgoing;
            }
        }
    }

    private MapPosition GetCurrentCell()
    {
        return new MapPosition(
            (int)MathF.Floor(Position.X),
            (int)MathF.Floor(Position.Y));
    }

    private TrackConnections GetOutgoingConnection(
        TrackCell track)
    {
        var opposite =
            GetOppositeConnection(
                _direction);

        var connections =
            track.Connections;

        var available =
            connections &
            ~opposite;

        if (available.HasFlag(
                _direction))
        {
            return _direction;
        }

        if (available.HasFlag(
                TrackConnections.North))
        {
            return TrackConnections.North;
        }

        if (available.HasFlag(
                TrackConnections.East))
        {
            return TrackConnections.East;
        }

        if (available.HasFlag(
                TrackConnections.South))
        {
            return TrackConnections.South;
        }

        if (available.HasFlag(
                TrackConnections.West))
        {
            return TrackConnections.West;
        }

        return TrackConnections.None;
    }

    private static TrackConnections
        GetOppositeConnection(
            TrackConnections connection)
    {
        return connection switch
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

    private static MapPosition GetNeighbour(
        MapPosition position,
        TrackConnections direction)
    {
        return direction switch
        {
            TrackConnections.North =>
                new MapPosition(
                    position.X,
                    position.Y - 1),

            TrackConnections.East =>
                new MapPosition(
                    position.X + 1,
                    position.Y),

            TrackConnections.South =>
                new MapPosition(
                    position.X,
                    position.Y + 1),

            TrackConnections.West =>
                new MapPosition(
                    position.X - 1,
                    position.Y),

            _ =>
                position
        };
    }

    private static Vector2 GetCellCenter(
        MapPosition position)
    {
        return new Vector2(
            position.X + 0.5f,
            position.Y + 0.5f);
    }
}