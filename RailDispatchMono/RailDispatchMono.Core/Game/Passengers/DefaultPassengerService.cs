using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System.Linq;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Default station passenger service. Alighting is performed before boarding.
/// Each wagon makes its own capacity and route decision.
/// Passenger ownership is tied to the concrete wagon, not to the current train.
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
        int before = 0;
        foreach (var vehicle in train.Composition.Vehicles)
        {
            if (vehicle is Wagon passengerWagon)
                before += _passengerManager.GetOnBoard(passengerWagon)
                    .Count(p => p.DestinationStation.Id == station.Id);
        }

        _passengerManager.AlightPassengers(train, station);
        int boarded = _passengerManager.BoardPassengers(train, station);
        return new PassengerServiceResult(boarded, before);
    }
}
