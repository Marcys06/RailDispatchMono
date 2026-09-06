using System;
using System.Collections.Generic;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Save;

/// <summary>Serializable DTOs for the 0.2.5 infrastructure save format.</summary>
public sealed class MapSaveData
{
    public int SchemaVersion { get; set; } = 3;
    public string GameVersion { get; set; } = "0.2.5";
    public MapInfoSaveData Map { get; set; } = new();
    public List<TrackSaveData> Tracks { get; set; } = new();
    public List<RailwayLineSaveData> RailwayLines { get; set; } = new();
    public List<SignalSaveData> Signals { get; set; } = new();
    public List<StationSaveData> Stations { get; set; } = new();
    public List<DepotSaveData> Depots { get; set; } = new();
}

public sealed class MapInfoSaveData { public int Width { get; set; } public int Height { get; set; } }

public sealed class TrackSaveData
{
    public int X { get; set; }
    public int Y { get; set; }
    public TrackGeometry Geometry { get; set; }
    public TrackConnections Connections { get; set; }
    public SwitchPosition SwitchPosition { get; set; }
    public TrackConnections CommonStem { get; set; }
    public TrackConnections StraightConnection { get; set; }
    public TrackConnections DivergingConnection { get; set; }
    public TrackType Type { get; set; } = TrackType.Mainline;
    public LineClass LineClass { get; set; } = LineClass.Mainline;
    public TractionSystem Traction { get; set; } = TractionSystem.None;
    public float WearPercent { get; set; }
}

public sealed class RailwayLineSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int ColorIndex { get; set; }
    public List<MapPositionSaveData> Tracks { get; set; } = new();
}

public sealed class MapPositionSaveData
{
    public int X { get; set; }
    public int Y { get; set; }
}

public sealed class SignalSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = ""
    ;
    public int X { get; set; }
    public int Y { get; set; }
    public TrackConnections Direction { get; set; }
    public SignalAspect Aspect { get; set; }
    public List<SignalAspect> AvailableAspects { get; set; } = new();
    public bool IsLocked { get; set; }
}

public sealed class StationSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float StopRadius { get; set; }
    public float DwellTimeSeconds { get; set; }
    public bool PassengerServiceEnabled { get; set; }
    public bool PassengerGenerationEnabled { get; set; }
    public float PassengerGenerationIntervalSeconds { get; set; }
    public int PassengerGenerationBatchSize { get; set; }
    public int PassengerWaitingCapacity { get; set; }
}

public sealed class DepotSaveData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
}
