using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private LocomotiveScheduleRuntime? _locomotiveScheduleRuntime;

    public LocomotiveSchedule? LocomotiveSchedule => Composition.Locomotive?.Schedule;
    public LocomotiveScheduleRuntime? LocomotiveScheduleRuntime => LocomotiveSchedule == null ? null : _locomotiveScheduleRuntime ??= new LocomotiveScheduleRuntime();

    /// <summary>Applies timetable departure control without taking switch/signal control away from the player.</summary>
    public bool PrepareAutomaticSchedule(StationController stations)
    {
        var schedule = LocomotiveSchedule;
        if (schedule == null || !schedule.Enabled || !schedule.IsValid(out _)) return false;
        var runtime = LocomotiveScheduleRuntime!;
        if (runtime.ScheduleId != schedule.Id) runtime.Reset(schedule.Id);

        int now = GameClock.Current == null ? 0 : (int)GameClock.Current.Seconds;
        var station = stations.GetStationAt(GetCurrentCell());
        if (station != null)
        {
            int pointIndex = schedule.Points.FindIndex(p => p.StationId == station.Id);
            if (pointIndex >= 0 && runtime.CurrentPointIndex != pointIndex)
            {
                runtime.RecordArrival(schedule, pointIndex, now, GameClock.Current?.GameDay ?? 1);
                Speed = 0f;
                return true;
            }
        }

        if (runtime.CurrentPointIndex >= 0 && runtime.State == LocomotiveScheduleState.WaitingAtStation)
        {
            int requiredDeparture = runtime.GetExpectedDeparture(schedule, runtime.CurrentPointIndex);
            if (now < requiredDeparture)
            {
                Speed = 0f;
                return true;
            }
            runtime.RecordDeparture(schedule, runtime.CurrentPointIndex, now);
        }

        runtime.State = LocomotiveScheduleState.Running;
        return false;
    }

    public void UpdateAutomaticScheduleState(StationController stations)
    {
        var schedule = LocomotiveSchedule;
        var runtime = LocomotiveScheduleRuntime;
        if (schedule == null || runtime == null || runtime.CurrentPointIndex < 0) return;
        var station = stations.GetStationAt(GetCurrentCell());
        if (station == null || schedule.Points[runtime.CurrentPointIndex].StationId != station.Id) return;
        int now = GameClock.Current == null ? 0 : (int)GameClock.Current.Seconds;
        runtime.RecordArrival(schedule, runtime.CurrentPointIndex, now, GameClock.Current?.GameDay ?? 1);
        Speed = 0f;
    }
}
