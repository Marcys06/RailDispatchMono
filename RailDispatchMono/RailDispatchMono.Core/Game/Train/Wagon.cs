using RailDispatchMono.Core.Game.Passengers;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Wagon : Vehicle
{
    private readonly List<Passenger> _passengers = new();

    public WagonType WagonType { get; set; }
    public int PassengerCapacity { get; }
    public IReadOnlyList<Passenger> Passengers => _passengers;
    public TrainRoute Route { get; }
    public IReadOnlyList<Guid> ServiceRoute => Route.StationIds;
    public int PassengerCount => _passengers.Count;
    public int AvailablePassengerCapacity => Math.Max(0, PassengerCapacity - _passengers.Count);

    public Wagon(
        VehicleParameters parameters,
        WagonType wagonType = WagonType.Passenger,
        int passengerCapacity = 80,
        IEnumerable<Guid>? serviceRoute = null)
        : base(parameters)
    {
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
        // compatibility with test consists. Once a route exists, only passengers
        // whose destination is one of its stops may board.
        return Route.IsEmpty || Route.ServesStation(passenger.DestinationStation.Id);
    }

    internal bool TryBoard(Passenger passenger, Guid trainId)
    {
        if (!CanAcceptPassenger(passenger))
            return false;

        _passengers.Add(passenger);
        passenger.State = PassengerState.OnBoard;
        passenger.CurrentTrainId = trainId;
        passenger.CurrentStationId = null;
        return true;
    }

    internal int TryAlightAt(Station station, Guid trainId)
    {
        int alighted = 0;
        for (int i = _passengers.Count - 1; i >= 0; i--)
        {
            var passenger = _passengers[i];
            if (passenger.DestinationStation.Id != station.Id || passenger.CurrentTrainId != trainId)
                continue;

            _passengers.RemoveAt(i);
            passenger.State = PassengerState.Arrived;
            passenger.CurrentTrainId = null;
            passenger.CurrentStationId = station.Id;
            alighted++;
        }

        Route.AdvanceToStation(station.Id);
        return alighted;
    }
}
