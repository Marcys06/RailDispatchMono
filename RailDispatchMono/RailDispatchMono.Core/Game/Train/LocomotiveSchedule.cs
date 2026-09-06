using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Operational timetable owned by a locomotive. Wagon schedules remain independent.</summary>
public sealed class LocomotiveSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Nowy rozkład lokomotywy";
    public List<LocomotiveSchedulePoint> Points { get; set; } = new();
    public bool Enabled { get; set; } = true;

    public int CycleDurationSeconds => Points.Count < 2 ? 0 : Points[^1].DepartureSeconds - Points[0].ArrivalSeconds;

    public bool IsValid(out string error)
    {
        if (Points.Count < 2) { error = "Rozkład lokomotywy wymaga co najmniej dwóch punktów."; return false; }
        for (int i = 0; i < Points.Count; i++)
        {
            var point = Points[i];
            if (point.StationId == Guid.Empty) { error = $"Punkt {i + 1} nie ma przypisanej stacji."; return false; }
            if (point.ArrivalSeconds < 0 || point.ArrivalSeconds >= 86400 || point.DepartureSeconds < 0 || point.DepartureSeconds >= 86400)
            { error = "Czasy muszą mieścić się w zakresie 00:00-23:59:59."; return false; }
            if (point.DepartureSeconds < point.ArrivalSeconds) { error = $"Odjazd w punkcie {i + 1} jest wcześniejszy niż przyjazd."; return false; }
            if (i > 0 && point.ArrivalSeconds < Points[i - 1].DepartureSeconds)
            { error = $"Czas punktu {i + 1} nie może być wcześniejszy od poprzedniego odjazdu."; return false; }
        }
        if (CycleDurationSeconds <= 0) { error = "Rozkład lokomotywy musi zawierać dodatni czas pełnego cyklu."; return false; }
        error = string.Empty;
        return true;
    }

    public LocomotiveSchedule Clone() => new()
    {
        Id = Id, Name = Name, Enabled = Enabled,
        Points = Points.Select(p => new LocomotiveSchedulePoint(p.StationId, p.ArrivalSeconds, p.DepartureSeconds)).ToList()
    };
}

public sealed class LocomotiveSchedulePoint
{
    public Guid StationId { get; set; }
    public int ArrivalSeconds { get; set; }
    public int DepartureSeconds { get; set; }
    public LocomotiveSchedulePoint() { }
    public LocomotiveSchedulePoint(Guid stationId, int arrivalSeconds, int departureSeconds)
    { StationId = stationId; ArrivalSeconds = arrivalSeconds; DepartureSeconds = departureSeconds; }
}

public enum LocomotiveScheduleState
{
    NotStarted,
    Running,
    WaitingAtStation,
    WaitingForSignal,
    WaitingForBlock,
    Completed
}

public sealed class LocomotiveScheduleRuntime
{
    public Guid ScheduleId { get; set; }
    public int CycleNumber { get; set; }
    public int CurrentPointIndex { get; set; } = -1;
    public LocomotiveScheduleState State { get; set; } = LocomotiveScheduleState.NotStarted;
    public int DelaySeconds { get; set; }
    public int LastObservedArrivalSeconds { get; set; } = -1;
    public int LastObservedDay { get; set; } = -1;
    public int RequiredDepartureSeconds { get; set; } = -1;

    public void Reset(Guid scheduleId)
    {
        ScheduleId = scheduleId; CycleNumber = 0; CurrentPointIndex = -1;
        State = LocomotiveScheduleState.NotStarted; DelaySeconds = 0;
        LastObservedArrivalSeconds = -1; LastObservedDay = -1; RequiredDepartureSeconds = -1;
    }

    public int GetScheduledArrival(LocomotiveSchedule schedule, int pointIndex) =>
        schedule.Points[pointIndex].ArrivalSeconds + CycleNumber * schedule.CycleDurationSeconds;

    public int GetScheduledDeparture(LocomotiveSchedule schedule, int pointIndex) =>
        schedule.Points[pointIndex].DepartureSeconds + CycleNumber * schedule.CycleDurationSeconds;

    public void RecordArrival(LocomotiveSchedule schedule, int pointIndex, int actualSeconds, int day)
    {
        if (pointIndex < 0 || pointIndex >= schedule.Points.Count) return;
        if (pointIndex == 0 && CurrentPointIndex == schedule.Points.Count - 1) CycleNumber++;
        CurrentPointIndex = pointIndex;
        DelaySeconds = actualSeconds - GetScheduledArrival(schedule, pointIndex);
        LastObservedArrivalSeconds = actualSeconds;
        LastObservedDay = day;
        RequiredDepartureSeconds = GetScheduledDeparture(schedule, pointIndex);
        State = pointIndex == schedule.Points.Count - 1 ? LocomotiveScheduleState.Completed : LocomotiveScheduleState.WaitingAtStation;
    }
}
