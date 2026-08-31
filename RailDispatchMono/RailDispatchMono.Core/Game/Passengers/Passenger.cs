using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Quasi-individual passenger. A passenger has a fixed origin and destination
/// station. Transfers are deliberately not performed in the first version.
/// </summary>
public sealed class Passenger
{
    public Guid Id { get; }
    public Station OriginStation { get; }
    public Station DestinationStation { get; }
    public PassengerState State { get; internal set; }
    public Guid? CurrentTrainId { get; internal set; }
    public DateTime CreatedAtUtc { get; }

    public Passenger(Station originStation, Station destinationStation)
    {
        if (originStation == null) throw new ArgumentNullException(nameof(originStation));
        if (destinationStation == null) throw new ArgumentNullException(nameof(destinationStation));
        if (originStation.Id == destinationStation.Id)
            throw new ArgumentException("Origin and destination stations must differ.");

        Id = Guid.NewGuid();
        OriginStation = originStation;
        DestinationStation = destinationStation;
        State = PassengerState.WaitingAtStation;
        CreatedAtUtc = DateTime.UtcNow;
    }
}
