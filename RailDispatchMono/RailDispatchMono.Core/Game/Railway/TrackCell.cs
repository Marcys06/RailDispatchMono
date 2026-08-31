using RailDispatchMono.Core.Game.Map;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Railway;

public enum SwitchPosition
{
    Straight = 0,
    Diverging = 1
}

public sealed class TrackCell
{
    public MapPosition Position { get; }
    public TrackGeometry Geometry { get; private set; }
    public TrackConnections Connections { get; private set; }
    public SwitchPosition CurrentSwitchPosition { get; private set; } = SwitchPosition.Straight;
    public TrackConnections StraightConnection { get; private set; } = TrackConnections.None;
    public TrackConnections DivergingConnection { get; private set; } = TrackConnections.None;
    public TrackConnections CommonStem { get; private set; } = TrackConnections.None;
    public bool IsJunction => Geometry == TrackGeometry.Junction;
    public bool IsSwitchedToDiverging => CurrentSwitchPosition == SwitchPosition.Diverging;
    public TrackConnections StraightSide => StraightConnection;
    public TrackConnections DivergingSide => DivergingConnection;
    public TrackConnections StemSide => CommonStem;

    public TrackCell(MapPosition position, TrackGeometry geometry, TrackConnections connections)
    {
        Position = position;
        Geometry = geometry;
        Connections = connections;
    }

    public void SetGeometry(TrackGeometry geometry) => Geometry = geometry;
    public void SetConnections(TrackConnections connections) => Connections = connections;

    public void SetSwitchPosition(SwitchPosition position)
    {
        if (Geometry == TrackGeometry.Junction)
            CurrentSwitchPosition = position;
    }

    public bool HasConnection(TrackConnections connection) => Connections.HasFlag(connection);

    public void ConfigureJunction(TrackConnections commonStem, TrackConnections straightExit, TrackConnections divergingExit)
    {
        Geometry = TrackGeometry.Junction;
        CommonStem = commonStem;
        StraightConnection = straightExit;
        DivergingConnection = divergingExit;
        Connections = commonStem | straightExit | divergingExit;
    }

    public void ToggleSwitch()
    {
        if (Geometry != TrackGeometry.Junction) return;
        CurrentSwitchPosition = CurrentSwitchPosition == SwitchPosition.Straight
            ? SwitchPosition.Diverging
            : SwitchPosition.Straight;
    }

    public TrackConnections GetExitDirection(TrackConnections entrySide)
    {
        if (Geometry != TrackGeometry.Junction)
            return Connections & ~entrySide;

        if (entrySide == CommonStem)
            return CurrentSwitchPosition == SwitchPosition.Straight ? StraightConnection : DivergingConnection;

        return CommonStem;
    }

    public List<TrackConnections> GetAvailableDirections()
    {
        var result = new List<TrackConnections>();
        if (Connections.HasFlag(TrackConnections.North)) result.Add(TrackConnections.North);
        if (Connections.HasFlag(TrackConnections.East)) result.Add(TrackConnections.East);
        if (Connections.HasFlag(TrackConnections.South)) result.Add(TrackConnections.South);
        if (Connections.HasFlag(TrackConnections.West)) result.Add(TrackConnections.West);
        return result;
    }
}
