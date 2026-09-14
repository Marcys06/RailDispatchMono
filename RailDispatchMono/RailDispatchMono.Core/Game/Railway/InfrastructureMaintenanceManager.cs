using RailDispatchMono.Core.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Runtime infrastructure maintenance and wear simulation. Wear is persisted by MapSaveService.</summary>
public sealed class InfrastructureMaintenanceManager
{
    private readonly GameMap _map;
    private readonly Dictionary<MapPosition, double> _wearRemainders = new();

    public const float WarningThresholdPercent = 60f;
    public const float CriticalThresholdPercent = 85f;
    public const float SevereThresholdPercent = 95f;

    public double SimulatedHours { get; private set; }
    public static InfrastructureMaintenanceManager? Current { get; private set; }

    public InfrastructureMaintenanceManager(GameMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        Current = this;
    }

    /// <summary>Advances wear using simulation time. Traffic and existing wear both affect degradation.</summary>
    public int Update(float simulationSeconds, IEnumerable<MapPosition>? occupiedTracks = null)
    {
        if (simulationSeconds <= 0f) return 0;

        double hours = simulationSeconds / 3600d;
        SimulatedHours += hours;
        var occupied = occupiedTracks == null ? new HashSet<MapPosition>() : occupiedTracks.ToHashSet();
        int changed = 0;

        foreach (var entry in _map.GetAllTracks())
        {
            TrackCell track = entry.Value;
            if (track.WearPercent >= 100f) continue;

            double ratePerHour = WearRatePerHour(track);
            double trafficMultiplier = occupied.Contains(entry.Key) ? 1d : 0.04d;
            double amount = ratePerHour * hours * trafficMultiplier * DegradationMultiplier(track.WearPercent);
            if (amount <= 0d) continue;

            double remainder = _wearRemainders.TryGetValue(entry.Key, out var previous) ? previous : 0d;
            double total = remainder + amount;
            if (total >= 0.001d)
            {
                track.ApplyWear((float)total);
                _wearRemainders[entry.Key] = 0d;
                changed++;
            }
            else
            {
                _wearRemainders[entry.Key] = total;
            }
        }

        return changed;
    }

    public int RepairAll() => RepairWhere(_ => true);

    public int RepairCritical() => RepairWhere(t => t.WearPercent >= CriticalThresholdPercent);

    public int RepairSevere() => RepairWhere(t => t.WearPercent >= SevereThresholdPercent);

    public int RepairWhere(Func<TrackCell, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        int repaired = 0;
        foreach (var entry in _map.GetAllTracks())
        {
            if (!predicate(entry.Value) || entry.Value.WearPercent <= 0f) continue;
            entry.Value.Repair();
            _wearRemainders.Remove(entry.Key);
            repaired++;
        }
        return repaired;
    }

    public IReadOnlyList<TrackConditionSummary> GetWorstTracks(int limit = 10)
    {
        if (limit <= 0) return Array.Empty<TrackConditionSummary>();
        return _map.GetAllTracks()
            .Select(x => ToSummary(x.Key, x.Value))
            .OrderByDescending(x => x.PriorityScore)
            .ThenByDescending(x => x.WearPercent)
            .ThenBy(x => x.Position.Y)
            .ThenBy(x => x.Position.X)
            .Take(limit)
            .ToList();
    }

    public IReadOnlyList<TrackConditionSummary> GetTracksByState(MaintenanceState state)
        => _map.GetAllTracks()
            .Select(x => ToSummary(x.Key, x.Value))
            .Where(x => x.State == state)
            .OrderByDescending(x => x.PriorityScore)
            .ThenBy(x => x.Position.Y)
            .ThenBy(x => x.Position.X)
            .ToList();

    public TrackConditionSummary? GetMostUrgentTrack()
    {
        if (_map.TrackCount == 0) return null;
        return _map.GetAllTracks()
            .Select(x => ToSummary(x.Key, x.Value))
            .OrderByDescending(x => x.PriorityScore)
            .ThenByDescending(x => x.WearPercent)
            .First();
    }

    public MaintenanceSummary GetSummary()
    {
        int total = 0, warning = 0, critical = 0, severe = 0;
        float wear = 0f;
        foreach (var entry in _map.GetAllTracks())
        {
            total++;
            wear += entry.Value.WearPercent;
            if (entry.Value.WearPercent >= SevereThresholdPercent) severe++;
            else if (entry.Value.WearPercent >= CriticalThresholdPercent) critical++;
            else if (entry.Value.WearPercent >= WarningThresholdPercent) warning++;
        }

        return new MaintenanceSummary(
            total,
            warning,
            critical,
            severe,
            total == 0 ? 100f : 100f - wear / total,
            SimulatedHours);
    }

    public static MaintenanceState ConditionStateFor(float wearPercent)
        => wearPercent >= CriticalThresholdPercent ? MaintenanceState.Critical
         : wearPercent >= WarningThresholdPercent ? MaintenanceState.Warning
         : MaintenanceState.Good;

    /// <summary>Operational recommendation used by future movement/dispatcher constraints.</summary>
    public static float RecommendedSpeedMultiplier(float wearPercent)
        => wearPercent >= SevereThresholdPercent ? 0.50f
         : wearPercent >= CriticalThresholdPercent ? 0.65f
         : wearPercent >= WarningThresholdPercent ? 0.85f
         : 1.0f;

    private static double DegradationMultiplier(float wearPercent)
        => wearPercent >= SevereThresholdPercent ? 2.5d
         : wearPercent >= CriticalThresholdPercent ? 1.7d
         : wearPercent >= WarningThresholdPercent ? 1.2d
         : 1.0d;

    private static TrackConditionSummary ToSummary(MapPosition position, TrackCell track)
    {
        float priority = track.WearPercent * 0.7f
            + (track.Type == TrackType.Mainline ? 15f : 0f)
            + (track.LineClass == LineClass.Magistral ? 15f : 0f)
            + (track.IsJunction ? 10f : 0f);
        return new TrackConditionSummary(
            position, track.WearPercent, track.ConditionPercent,
            ConditionStateFor(track.WearPercent), track.Type,
            track.LineClass, track.Traction, priority);
    }

    private static double WearRatePerHour(TrackCell track)
    {
        double lineFactor = track.LineClass switch
        {
            LineClass.Local => 0.18d,
            LineClass.Regional => 0.22d,
            LineClass.Mainline => 0.28d,
            LineClass.Magistral => 0.34d,
            _ => 0.2d
        };
        double typeFactor = track.Type switch
        {
            TrackType.Mainline => 1.15d,
            TrackType.Secondary => 0.9d,
            TrackType.Siding => 0.45d,
            TrackType.Platform => 0.6d,
            _ => 1d
        };
        double tractionFactor = track.Traction == TractionSystem.None ? 0.95d : 1.05d;
        return lineFactor * typeFactor * tractionFactor;
    }
}

public enum MaintenanceState { Good, Warning, Critical }

public readonly record struct TrackConditionSummary(
    MapPosition Position,
    float WearPercent,
    float ConditionPercent,
    MaintenanceState State,
    TrackType Type,
    LineClass LineClass,
    TractionSystem Traction,
    float PriorityScore)
{
    public float RecommendedSpeedMultiplier => InfrastructureMaintenanceManager.RecommendedSpeedMultiplier(WearPercent);
}

public readonly record struct MaintenanceSummary(
    int TrackCount,
    int WarningTracks,
    int CriticalTracks,
    int SevereTracks,
    float AverageConditionPercent,
    double SimulatedHours);
