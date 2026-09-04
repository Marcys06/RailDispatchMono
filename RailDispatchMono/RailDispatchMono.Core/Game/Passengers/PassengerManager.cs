using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

using TrainClass = RailDispatchMono.Core.Game.Train.Train;

namespace RailDispatchMono.Core.Game.Passengers;

public readonly record struct PassengerExchange(TrainClass Train, Wagon Wagon, int WagonIndex, int Boarded, int Alighted);

public sealed class PassengerManager
{
    private readonly List<Passenger> _passengers = new();
    public IReadOnlyList<Passenger> Passengers => _passengers;
    public int Count => _passengers.Count;

    public event Action<PassengerExchange>? PassengerExchangeOccurred;

    public Passenger CreatePassenger(Station origin, Station destination)
    {
        var passenger = new Passenger(origin, destination);
        _passengers.Add(passenger);
        return passenger;
    }

    public IEnumerable<Passenger> GetWaitingAt(Station station) =>
        _passengers.Where(p => p.State == PassengerState.WaitingAtStation && p.CurrentStationId == station.Id);

    /// <summary>
    /// Returns passengers physically inside wagons that are currently part of
    /// the supplied train. The train is only an operational view; passenger
    /// ownership remains tied to the concrete wagon.
    /// </summary>
    public IEnumerable<Passenger> GetOnBoard(TrainClass train) =>
        train.Composition.Vehicles
            .OfType<Wagon>()
            .SelectMany(GetOnBoard);

    public IEnumerable<Passenger> GetOnBoard(Wagon wagon) =>
        _passengers.Where(p => p.State == PassengerState.OnBoard && p.CurrentWagonId == wagon.Id);

    /// <summary>
    /// Returns passengers whose current wagon can no longer serve their
    /// destination from its current route position. This is a transfer-ready
    /// diagnostic hook; it does not select a train and does not move passengers.
    /// </summary>
    public IEnumerable<Passenger> GetTransferCandidates(TrainClass train) =>
        train.Composition.Vehicles
            .OfType<Wagon>()
            .SelectMany(GetOnBoard)
            .Where(p =>
            {
                var wagon = train.Composition.Vehicles
                    .OfType<Wagon>()
                    .FirstOrDefault(w => w.Id == p.CurrentWagonId);
                return wagon != null && !wagon.CanContinueJourneyTo(p.DestinationStation.Id);
            });

    public int GetWaitingCount(Station station) => GetWaitingAt(station).Count();
    public int GetOnBoardCount(TrainClass train) => GetOnBoard(train).Count();
    public int GetOnBoardCount(Wagon wagon) => GetOnBoard(wagon).Count();

    public int BoardPassengers(TrainClass train, Station station)
    {
        int boarded = 0;
        var waiting = GetWaitingAt(station).ToList();

        for (int wagonIndex = 0; wagonIndex < train.Composition.Vehicles.Count; wagonIndex++)
        {
            if (train.Composition.Vehicles[wagonIndex] is not Wagon wagon)
                continue;

            int wagonBoarded = 0;
            foreach (var passenger in waiting)
            {
                if (!wagon.CanAcceptPassenger(passenger, station))
                    continue;

                if (wagon.TryBoard(passenger, station))
                {
                    boarded++;
                    wagonBoarded++;
                }
            }

            if (wagonBoarded > 0)
            {
                var exchange = new PassengerExchange(train, wagon, wagonIndex, wagonBoarded, 0);
                PassengerExchangeOccurred?.Invoke(exchange);
                FloatingTextManager.NotifyPassengerExchange(exchange);
            }
        }

        return boarded;
    }

    public int AlightPassengers(TrainClass train, Station station)
    {
        int alighted = 0;

        for (int wagonIndex = 0; wagonIndex < train.Composition.Vehicles.Count; wagonIndex++)
        {
            if (train.Composition.Vehicles[wagonIndex] is not Wagon wagon)
                continue;

            int wagonAlighted = wagon.TryAlightAt(station);
            alighted += wagonAlighted;

            if (wagonAlighted > 0)
            {
                var exchange = new PassengerExchange(train, wagon, wagonIndex, 0, wagonAlighted);
                PassengerExchangeOccurred?.Invoke(exchange);
                FloatingTextManager.NotifyPassengerExchange(exchange);
            }
        }

        return alighted;
    }

    /// <summary>
    /// Registers a passenger restored from persistence in the authoritative
    /// passenger manager and attaches the same passenger instance to the wagon.
    /// </summary>
    public bool RestoreOnBoard(Wagon wagon, Passenger passenger)
    {
        if (wagon == null || passenger == null || _passengers.Contains(passenger))
            return false;

        if (!wagon.RestorePassenger(passenger))
            return false;

        _passengers.Add(passenger);
        return true;
    }

    public void RemoveCompletedPassengers() => _passengers.RemoveAll(p => p.State == PassengerState.Arrived);
    public void Clear() => _passengers.Clear();
}
