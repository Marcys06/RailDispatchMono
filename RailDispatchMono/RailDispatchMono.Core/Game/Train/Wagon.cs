using RailDispatchMono.Core.Game.Passengers;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Wagon : Vehicle
{
    private readonly List<Passenger> _passengers = new();

    public string ShortName { get; }
    public WagonType WagonType { get; set; }
    public int PassengerCapacity { get; }
    public IReadOnlyList<Passenger> Passengers => _passengers;
    public TrainRoute Route { get; }
    public IReadOnlyList<Guid> ServiceRoute => Route.StationIds;
    public int PassengerCount => _passengers.Count;
    public int AvailablePassengerCapacity => Math.Max(0, PassengerCapacity - _passengers.Count);

    public Wagon(
        VehicleParameters parameters,
        string shortName,
        WagonType wagonType = WagonType.Passenger,
        int passengerCapacity = 80,
        IEnumerable<Guid>? serviceRoute = null)
        : base(parameters)
    {
        ShortName = shortName;
        WagonType = wagonType;
        PassengerCapacity = Math.Max(0, passengerCapacity);
        Route = new TrainRoute();
        if (serviceRoute != null)
        {
            foreach (var stationId in serviceRoute)
                Route.AddStation(stationId);
        }
    }

    public bool CanAcceptPassenger(Passenger passenger)
    {
        if (passenger == null || WagonType != WagonType.Passenger || AvailablePassengerCapacity <= 0)
            return false;

        // An empty route is intentionally treated as "not yet configured" for
        // compatibility with test consists. Once a route exists, passengers
        // whose destination has already been passed are not allowed to board.
        return Route.IsEmpty || Route.CanServeStation(passenger.DestinationStation.Id);
    }

    internal bool TryBoard(Passenger passenger)
    {
        if (!CanAcceptPassenger(passenger))
            return false;

        _passengers.Add(passenger);
        passenger.State = PassengerState.OnBoard;
        passenger.CurrentWagonId = Id;
        passenger.CurrentStationId = null;
        return true;
    }

    internal int TryAlightAt(Station station)
    {
        int alighted = 0;
        for (int i = _passengers.Count - 1; i >= 0; i--)
        {
            var passenger = _passengers[i];
            if (passenger.DestinationStation.Id != station.Id)
                continue;

            _passengers.RemoveAt(i);
            passenger.State = PassengerState.Arrived;
            passenger.CurrentWagonId = null;
            passenger.CurrentStationId = station.Id;
            alighted++;
        }

        Route.AdvanceToStation(station.Id);
        return alighted;
    }
}
