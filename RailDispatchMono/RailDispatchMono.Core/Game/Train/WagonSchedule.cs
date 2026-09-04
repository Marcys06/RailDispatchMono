using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public enum WagonScheduleState
{
    NotStarted,
    EnRoute,
    AtStation,
    Completed
}

public sealed class WagonSchedulePoint
{
    public Guid StationId { get; set; }
    public int ArrivalSeconds { get; set; }
    public int DepartureSeconds { get; set; }

    public WagonSchedulePoint() { }

    public WagonSchedulePoint(Guid stationId, int arrivalSeconds, int departureSeconds)
    {
        StationId = stationId;
        ArrivalSeconds = arrivalSeconds;
        DepartureSeconds = departureSeconds;
    }
}

public sealed class WagonSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Nowy rozkład";
    public List<Guid> BaseStationIds { get; set; } = new();
    public List<WagonSchedulePoint> Points { get; set; } = new();
    public bool Enabled { get; set; } = true;

    /// <summary>Length of one repetition from the first arrival to the final departure.</summary>
    public int CycleDurationSeconds => Points.Count < 2 ? 0 : Points[^1].DepartureSeconds - Points[0].ArrivalSeconds;

    public int GetScheduledTime(int pointIndex, int cycleNumber)
    {
        if (pointIndex < 0 || pointIndex >= Points.Count) throw new ArgumentOutOfRangeException(nameof(pointIndex));
        if (cycleNumber < 0) throw new ArgumentOutOfRangeException(nameof(cycleNumber));
        return Points[pointIndex].ArrivalSeconds - Points[0].ArrivalSeconds + cycleNumber * CycleDurationSeconds;
    }

    public bool IsValid(out string error)
    {
        if (BaseStationIds.Count < 2)
        {
            error = "Rozkład wymaga co najmniej dwóch stacji.";
            return false;
        }

        if (Points.Count != BaseStationIds.Count * 2 - 1)
        {
            error = "Liczba punktów rozkładu nie odpowiada pętli A-B-...-B-A.";
            return false;
        }

        for (int i = 0; i < Points.Count; i++)
        {
            var point = Points[i];
            if (point.ArrivalSeconds < 0 || point.ArrivalSeconds >= 24 * 60 * 60 ||
                point.DepartureSeconds < 0 || point.DepartureSeconds >= 24 * 60 * 60)
            {
                error = "Czasy muszą mieścić się w zakresie 00:00-23:59:59.";
                return false;
            }

            if (point.DepartureSeconds < point.ArrivalSeconds)
            {
                error = $"Odjazd w punkcie {i + 1} jest wcześniejszy niż przyjazd.";
                return false;
            }

            if (i > 0 && point.ArrivalSeconds < Points[i - 1].DepartureSeconds)
            {
                error = $"Czas punktu {i + 1} nie może być wcześniejszy od poprzedniego odjazdu.";
                return false;
            }
        }

        if (CycleDurationSeconds <= 0)
        {
            error = "Rozkład musi zawierać dodatni czas pełnego cyklu.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public void BuildLoopFromBaseRoute()
    {
        Points.Clear();
        if (BaseStationIds.Count < 2) return;

        // A-B-C becomes A-B-C-B-A. The terminal A is added once at the end;
        // the first occurrence remains the departure/initial terminal.
        var loop = BaseStationIds.Concat(BaseStationIds.Reverse().Skip(1)).ToList();
        for (int i = 0; i < loop.Count; i++)
            Points.Add(new WagonSchedulePoint(loop[i], 0, 0));
    }

    public WagonSchedule Clone()
    {
        return new WagonSchedule
        {
            Id = Id,
            Name = Name,
            Enabled = Enabled,
            BaseStationIds = BaseStationIds.ToList(),
            Points = Points.Select(p => new WagonSchedulePoint(p.StationId, p.ArrivalSeconds, p.DepartureSeconds)).ToList()
        };
    }
}

public sealed class WagonScheduleRuntime
{
    public Guid ScheduleId { get; set; }
    public int CycleNumber { get; set; }
    public int CurrentPointIndex { get; set; } = -1;
    public WagonScheduleState State { get; set; } = WagonScheduleState.NotStarted;
    public int DelaySeconds { get; set; }
    public int LastObservedArrivalSeconds { get; set; } = -1;
    public int LastObservedDay { get; set; } = -1;
    public int DwellUntilSeconds { get; set; } = -1;

    public void Reset(Guid scheduleId)
    {
        ScheduleId = scheduleId;
        CycleNumber = 0;
        CurrentPointIndex = -1;
        State = WagonScheduleState.NotStarted;
        DelaySeconds = 0;
        LastObservedArrivalSeconds = -1;
        LastObservedDay = -1;
        DwellUntilSeconds = -1;
    }

    public void RecordArrival(WagonSchedule schedule, int pointIndex, int actualSeconds, int day)
    {
        if (pointIndex < 0 || pointIndex >= schedule.Points.Count) return;
        var point = schedule.Points[pointIndex];
        if (pointIndex == 0 && State == WagonScheduleState.Completed)
            CycleNumber++;
        CurrentPointIndex = pointIndex;
        DelaySeconds = actualSeconds - (point.ArrivalSeconds + CycleNumber * schedule.CycleDurationSeconds);
        LastObservedArrivalSeconds = actualSeconds;
        LastObservedDay = day;
        DwellUntilSeconds = point.DepartureSeconds + CycleNumber * schedule.CycleDurationSeconds + DelaySeconds;
        State = pointIndex == schedule.Points.Count - 1 ? WagonScheduleState.Completed : WagonScheduleState.AtStation;
    }
}
