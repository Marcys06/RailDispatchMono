using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Linq;

// Alias zapobiegający konfliktowi CS0118 (Namespace vs Typ)
using TrainClass = RailDispatchMono.Core.Game.Train.Train;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Owns quasi-individual passengers and station queues.
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
        _passengers.Where(p => p.State == PassengerState.WaitingAtStation &&
                               p.CurrentStationId == station.Id);

    public IEnumerable<Passenger> GetOnBoard(TrainClass train) =>
        _passengers.Where(p => p.State == PassengerState.OnBoard && p.CurrentTrainId == train.Id);

    public int GetWaitingCount(Station station) => GetWaitingAt(station).Count();
    public int GetOnBoardCount(TrainClass train) => GetOnBoard(train).Count();

    public int BoardPassengers(TrainClass train, Station station)
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

    public int AlightPassengers(TrainClass train, Station station)
    {
        int alighted = 0;
        foreach (var vehicle in train.Composition.Vehicles)
        {
            if (vehicle is Wagon wagon)
                alighted += wagon.TryAlightAt(station, train.Id);
        }

        return alighted;
    }

    public void RemoveCompletedPassengers()
    {
        _passengers.RemoveAll(p => p.State == PassengerState.Arrived);
    }
}
