using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

public sealed class TrackCell
{
    public MapPosition Position { get; }

    public TrackGeometry Geometry { get; private set; }

    public TrackConnections Connections { get; private set; }

    public TrackCell(
        MapPosition position,
        TrackGeometry geometry,
        TrackConnections connections)
    {
        Position = position;
        Geometry = geometry;
        Connections = connections;
    }

    public void SetGeometry(
        TrackGeometry geometry)
    {
        Geometry = geometry;
    }

    public void SetConnections(
        TrackConnections connections)
    {
        Connections = connections;
    }

    public bool HasConnection(
        TrackConnections connection)
    {
        return Connections.HasFlag(connection);
    }
}









