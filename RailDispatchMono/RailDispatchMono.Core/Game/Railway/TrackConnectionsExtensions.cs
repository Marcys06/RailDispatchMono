namespace RailDispatchMono.Core.Game.Railway;

public static class TrackConnectionsExtensions
{
    public static TrackConnections GetOppositeDirection(this TrackConnections direction) => direction switch
    {
        TrackConnections.North => TrackConnections.South,
        TrackConnections.East => TrackConnections.West,
        TrackConnections.South => TrackConnections.North,
        TrackConnections.West => TrackConnections.East,
        _ => TrackConnections.None
    };
}
