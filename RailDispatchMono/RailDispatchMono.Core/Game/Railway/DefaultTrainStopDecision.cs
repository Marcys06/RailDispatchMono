using TrainNS = RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// First-generation stop policy: every passenger-service station is a stop.
/// A future timetable/route policy can replace this without changing station
/// or passenger code.
/// </summary>
public sealed class DefaultTrainStopDecision : ITrainStopDecision
{
    public bool ShouldStopAt(TrainNS.Train train, Station station)
    {
        return station.PassengerServiceEnabled && train.CanMove;
    }
}

