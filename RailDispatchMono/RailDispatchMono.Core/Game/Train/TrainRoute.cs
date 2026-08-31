using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>
/// JSON-ready route definition owned by a single wagon.
/// A route is an ordered list of station identifiers. It does not control the
/// locomotive; physical movement remains controlled by signals and switches.
/// </summary>
public sealed class TrainRoute
{
    public string Version { get; set; } = "1";
    public List<Guid> StationIds { get; set; } = new();
    public int CurrentStopIndex { get; set; }

    public bool IsEmpty => StationIds.Count == 0;
    public Guid? CurrentStationId =>
        CurrentStopIndex >= 0 && CurrentStopIndex < StationIds.Count ? StationIds[CurrentStopIndex] : null;
    public Guid? NextStationId =>
        CurrentStopIndex + 1 < StationIds.Count ? StationIds[CurrentStopIndex + 1] : null;

    public void AddStation(Guid stationId)
    {
        if (stationId == Guid.Empty || StationIds.Contains(stationId))
            return;
        StationIds.Add(stationId);
        if (CurrentStopIndex < 0)
            CurrentStopIndex = 0;
    }

    public bool RemoveStation(Guid stationId)
    {
        int index = StationIds.IndexOf(stationId);
        if (index < 0) return false;
        StationIds.RemoveAt(index);
        if (StationIds.Count == 0)
        {
            CurrentStopIndex = 0;
            return true;
        }
        if (index < CurrentStopIndex)
            CurrentStopIndex--;
        CurrentStopIndex = Math.Clamp(CurrentStopIndex, 0, StationIds.Count - 1);
        return true;
    }

    public void Clear()
    {
        StationIds.Clear();
        CurrentStopIndex = 0;
    }

    public void AdvanceToStation(Guid stationId)
    {
        int index = StationIds.IndexOf(stationId);
        if (index >= 0)
            CurrentStopIndex = index;
    }

    public bool ServesStation(Guid stationId) => StationIds.Contains(stationId);

    public string ToJson(bool indented = true) =>
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = indented });

    public static TrainRoute FromJson(string json)
    {
        var route = JsonSerializer.Deserialize<TrainRoute>(json);
        if (route == null)
            throw new InvalidOperationException("Invalid train route JSON.");
        route.StationIds ??= new List<Guid>();
        route.CurrentStopIndex = Math.Clamp(route.CurrentStopIndex, 0, Math.Max(0, route.StationIds.Count - 1));
        return route;
    }

    public static TrainRoute FromStations(IEnumerable<Station> stations)
    {
        var route = new TrainRoute();
        foreach (var station in stations ?? Enumerable.Empty<Station>())
            route.AddStation(station.Id);
        return route;
    }
}
