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

    /// <summary>
    /// Determines whether this concrete wagon can take the passenger at the
    /// specified station. A configured route must currently be at the station
    /// and must contain the passenger's destination at or after that stop.
    /// Empty routes remain permissive for backwards compatibility.
    /// </summary>
    public bool CanAcceptPassenger(Passenger passenger, Station station)
    {
        if (passenger == null || station == null ||
            WagonType != WagonType.Passenger || AvailablePassengerCapacity <= 0)
            return false;

        if (passenger.State != PassengerState.WaitingAtStation ||
            passenger.CurrentStationId != station.Id)
            return false;

        if (Route.IsEmpty)
            return true;

        return Route.CurrentStationId == station.Id &&
               Route.CanServeStation(passenger.DestinationStation.Id);
    }

    internal bool TryBoard(Passenger passenger, Station station)
    {
        if (!CanAcceptPassenger(passenger, station))
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
