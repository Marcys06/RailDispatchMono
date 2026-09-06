using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Simulation;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private LocomotiveScheduleRuntime? _locomotiveScheduleRuntime;

    public LocomotiveSchedule? LocomotiveSchedule => Composition.Locomotive?.Schedule;
    public LocomotiveScheduleRuntime? LocomotiveScheduleRuntime => _locomotiveScheduleRuntime;

    /// <summary>Applies timetable departure control without taking switch/signal control away from the player.</summary>
    public bool PrepareAutomaticSchedule(StationController stations)
    {
        var schedule = LocomotiveSchedule;
        if (schedule == null || !schedule.Enabled || !schedule.IsValid(out _)) return false;
        _locomotiveScheduleRuntime ??= new LocomotiveScheduleRuntime();
        if (_locomotiveScheduleRuntime.ScheduleId != schedule.Id) _locomotiveScheduleRuntime.Reset(schedule.Id);

        int now = GameClock.Current == null ? 0 : (int)GameClock.Current.Seconds;
        var station = stations.GetStationAt(GetCurrentCell());
        if (station != null)
        {
            int pointIndex = schedule.Points.FindIndex(p => p.StationId == station.Id);
            if (pointIndex >= 0 && _locomotiveScheduleRuntime.CurrentPointIndex != pointIndex)
            {
                _locomotiveScheduleRuntime.RecordArrival(schedule, pointIndex, now, GameClock.Current?.GameDay ?? 1);
                Speed = 0f;
                return true;
            }
        }

        if (_locomotiveScheduleRuntime.CurrentPointIndex >= 0 &&
            _locomotiveScheduleRuntime.State == LocomotiveScheduleState.WaitingAtStation)
        {
            if (now < _locomotiveScheduleRuntime.RequiredDepartureSeconds)
            {
                Speed = 0f;
                return true;
            }
            _locomotiveScheduleRuntime.State = LocomotiveScheduleState.Running;
        }

        _locomotiveScheduleRuntime.State = LocomotiveScheduleState.Running;
        return false;
    }

    public void UpdateAutomaticScheduleState(StationController stations)
    {
        var schedule = LocomotiveSchedule;
        var runtime = _locomotiveScheduleRuntime;
        if (schedule == null || runtime == null || runtime.CurrentPointIndex < 0) return;
        var station = stations.GetStationAt(GetCurrentCell());
        if (station == null || schedule.Points[runtime.CurrentPointIndex].StationId != station.Id) return;
        int now = GameClock.Current == null ? 0 : (int)GameClock.Current.Seconds;
        runtime.RecordArrival(schedule, runtime.CurrentPointIndex, now, GameClock.Current?.GameDay ?? 1);
        Speed = 0f;
    }
}
