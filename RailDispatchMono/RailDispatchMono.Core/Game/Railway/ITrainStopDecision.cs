using TrainNS = RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// Decides whether a train should perform a passenger stop at a station.
/// The station does not define a physical stopping point; semaphores remain
/// responsible for where a train is required to stop on the railway.
/// </summary>
public interface ITrainStopDecision
{
    bool ShouldStopAt(TrainNS.Train train, Station station);
}

