using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Persistent schedule document for one train. Wagon and locomotive timetables are independent.</summary>
public sealed class TrainSchedule
{
    public string Version { get; set; } = "2";
    public Guid TrainId { get; set; }
    public LocomotiveSchedule? Locomotive { get; set; }
    public List<WagonScheduleEntry> Wagons { get; set; } = new();
}

public sealed class WagonScheduleEntry
{
    public int WagonIndex { get; set; }
    public WagonType WagonType { get; set; }
    public List<Guid> StationIds { get; set; } = new();
}
