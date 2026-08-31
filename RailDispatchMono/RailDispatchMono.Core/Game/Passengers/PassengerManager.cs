using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Owns quasi-individual passengers and station queues. Routing is intentionally
/// direct in 0.0.10; transfers are represented by the WaitingAtStation state and
/// can be implemented later without changing the passenger identity model.
/// </summary>
public sealed class PassengerManager
{
    private readonly List<Passenger> _passengers = new();

    public IReadOnlyList<Passenger> Passengers => _passengers;
    public int Count => _passengers.Count;

    public Passenger CreatePassenger(Station origin, Station destination)
    {
        var passenger = new Passenger(origin, destination);
        _passengers.Add(passenger);
        return passenger;
    }

    public IEnumerable<Passenger> GetWaitingAt(Station station) =>
        _passengers.Where(p => p.State == PassengerState.WaitingAtStation && p.OriginStation.Id == station.Id);

    public IEnumerable<Passenger> GetOnBoard(Train train) =>
        _passengers.Where(p => p.State == PassengerState.OnBoard && p.CurrentTrainId == train.Id);

    public int BoardPassengers(Train train, Station station)
    {
        int boarded = 0;
        var waiting = GetWaitingAt(station).ToList();

        foreach (var passenger in waiting)
        {
            Wagon? selectedWagon = null;
            foreach (var vehicle in train.Composition.Vehicles)
            {
                if (vehicle is Wagon wagon && wagon.CanAcceptPassenger(passenger))
                {
                    selectedWagon = wagon;
                    break;
                }
            }

            if (selectedWagon == null)
                break;

            if (selectedWagon.TryBoard(passenger, train.Id))
                boarded++;
        }

        return boarded;
    }

    public int AlightPassengers(Train train, Station station)
    {
        int alighted = 0;
        foreach (var vehicle in train.Composition.Vehicles)
        {
            if (vehicle is Wagon wagon && wagon.TryAlightAt(station, train.Id))
            {
                // Count is derived below to keep Wagon responsible for its own list.
                alighted++;
            }
        }

        // One increment per wagon is not a passenger count; return the actual
        // delta from the manager state instead.
        return _passengers.Count(p => p.State == PassengerState.Arrived && p.DestinationStation.Id == station.Id);
    }

    public void RemoveCompletedPassengers()
    {
        _passengers.RemoveAll(p => p.State == PassengerState.Arrived);
    }
}
