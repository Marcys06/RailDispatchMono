using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System.Linq;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Default station passenger service. Alighting is performed before boarding.
/// Each wagon makes its own capacity and route decision.
/// </summary>
public sealed class DefaultPassengerService : IPassengerService
{
    private readonly PassengerManager _passengerManager;

    public DefaultPassengerService(PassengerManager passengerManager)
    {
        _passengerManager = passengerManager;
    }

    public PassengerServiceResult ServiceTrainAtStation(Train train, Station station)
    {
        int before = _passengerManager.Passengers.Count(p =>
            p.State == PassengerState.OnBoard && p.CurrentTrainId == train.Id &&
            p.DestinationStation.Id == station.Id);

        _passengerManager.AlightPassengers(train, station);
        int boarded = _passengerManager.BoardPassengers(train, station);
        return new PassengerServiceResult(boarded, before);
    }
}
