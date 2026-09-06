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

    /// <summary>Operational role of this track segment, independent of its geometric shape.</summary>
    public TrackType Type { get; private set; } = TrackType.Mainline;

    /// <summary>Infrastructure class used by future speed and operating rules.</summary>
    public LineClass LineClass { get; private set; } = LineClass.Mainline;

    /// <summary>Electrical system carried by this segment. None means non-electrified.</summary>
    public TractionSystem Traction { get; private set; } = TractionSystem.None;

    /// <summary>Wear percentage: 0 = new, 100 = fully worn. Simulation is introduced later.</summary>
    public float WearPercent { get; private set; }

    public bool IsJunction => Geometry == TrackGeometry.Junction;
    public bool IsSwitchedToDiverging => CurrentSwitchPosition == SwitchPosition.Diverging;
    public TrackConnections StraightSide => StraightConnection;
    public TrackConnections DivergingSide => DivergingConnection;
    public TrackConnections StemSide => CommonStem;
    public float ConditionPercent => 100f - WearPercent;
    public float InfrastructureMaxSpeedKmh => LineClassProfile.MaxSpeedKmh(LineClass);
    public float InfrastructureMaxAxleLoadTons => LineClassProfile.MaxAxleLoadTons(LineClass);

    public TrackCell(MapPosition position, TrackGeometry geometry, TrackConnections connections)
    {
        Position = position;
        Geometry = geometry;
        Connections = connections;
    }

    public void SetGeometry(TrackGeometry geometry) => Geometry = geometry;
    public void SetConnections(TrackConnections connections) => Connections = connections;

    public void SetInfrastructure(TrackType type, LineClass lineClass, TractionSystem traction)
    {
        Type = type;
        LineClass = lineClass;
        Traction = traction;
    }

    public void SetWear(float wearPercent)
    {
        WearPercent = System.Math.Clamp(wearPercent, 0f, 100f);
    }

    public void ApplyWear(float amount) => SetWear(WearPercent + amount);

    public void Repair() => WearPercent = 0f;

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
