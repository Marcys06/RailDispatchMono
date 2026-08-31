using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Handles passenger exchange between a train and a station.
/// Keeping this separate from stop decisions allows timetable and routing
/// rules to evolve independently from passenger handling.
/// </summary>
public interface IPassengerService
{
    PassengerServiceResult ServiceTrainAtStation(Train train, Station station);
}

public readonly record struct PassengerServiceResult(int Boarded, int Alighted);
