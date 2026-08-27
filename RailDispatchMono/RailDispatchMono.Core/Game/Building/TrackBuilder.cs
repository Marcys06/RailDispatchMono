using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Building;

public sealed class TrackBuilder
{
    private readonly GameMap _map;

    public TrackBuildMode Mode { get; set; } =
        TrackBuildMode.Straight;

    public CurveDirection Curve { get; set; } =
        CurveDirection.NorthEast;

    public bool StraightHorizontal { get; set; } = true;

    public TrackBuilder(GameMap map)
    {
        _map = map;
    }

    public void BuildStraight(
        MapPosition position,
        bool horizontal)
    {
        if (!IsInsideMap(position))
            return;

        var connections = horizontal
            ? TrackConnections.West |
              TrackConnections.East
            : TrackConnections.North |
              TrackConnections.South;

        var track = GetOrCreate(position);

        track.SetGeometry(
            TrackGeometry.Straight);

        track.SetConnections(
            connections);

        ConnectNeighbours(
            position,
            connections);
    }

    public void BuildCurve(
        MapPosition position,
        CurveDirection direction)
    {
        if (!IsInsideMap(position))
            return;

        var connections = direction switch
        {
            CurveDirection.NorthEast =>
                TrackConnections.North |
                TrackConnections.East,

            CurveDirection.EastSouth =>
                TrackConnections.East |
                TrackConnections.South,

            CurveDirection.SouthWest =>
                TrackConnections.South |
                TrackConnections.West,

            CurveDirection.WestNorth =>
                TrackConnections.West |
                TrackConnections.North,

            _ =>
                TrackConnections.None
        };

        var track = GetOrCreate(position);

        track.SetGeometry(
            TrackGeometry.Curve);

        track.SetConnections(
            connections);

        ConnectNeighbours(
            position,
            connections);
    }

    public void Remove(
        MapPosition position)
    {
        if (!IsInsideMap(position))
            return;

        _map.RemoveTrack(position);
    }

    private TrackCell GetOrCreate(
        MapPosition position)
    {
        if (_map.TryGetTrack(
                position,
                out var existing) &&
            existing is not null)
        {
            return existing;
        }

        if (!IsInsideMap(position))
        {
            throw new System.ArgumentOutOfRangeException(
                nameof(position));
        }

        var track = new TrackCell(
            position,
            TrackGeometry.Straight,
            TrackConnections.None);

        _map.AddTrack(track);

        return track;
    }

    private void ConnectNeighbours(
        MapPosition position,
        TrackConnections connections)
    {
        if (connections.HasFlag(
                TrackConnections.North))
        {
            AddConnection(
                position.X,
                position.Y - 1,
                TrackConnections.South);
        }

        if (connections.HasFlag(
                TrackConnections.East))
        {
            AddConnection(
                position.X + 1,
                position.Y,
                TrackConnections.West);
        }

        if (connections.HasFlag(
                TrackConnections.South))
        {
            AddConnection(
                position.X,
                position.Y + 1,
                TrackConnections.North);
        }

        if (connections.HasFlag(
                TrackConnections.West))
        {
            AddConnection(
                position.X - 1,
                position.Y,
                TrackConnections.East);
        }
    }

    private void AddConnection(
        int x,
        int y,
        TrackConnections connection)
    {
        var position =
            new MapPosition(x, y);

        if (!IsInsideMap(position))
            return;

        if (!_map.TryGetTrack(
                position,
                out var neighbour) ||
            neighbour is null)
        {
            return;
        }

        neighbour.SetConnections(
            neighbour.Connections |
            connection);
    }

    private bool IsInsideMap(
        MapPosition position)
    {
        return
            position.X >= 0 &&
            position.X < _map.Size.Width &&
            position.Y >= 0 &&
            position.Y < _map.Size.Height;
    }
}