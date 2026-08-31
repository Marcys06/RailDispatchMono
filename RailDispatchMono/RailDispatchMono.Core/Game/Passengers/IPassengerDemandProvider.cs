using RailDispatchMono.Core.Game.Railway;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Supplies passenger destinations for an origin station.
/// The default implementation is random; a city/population model can replace it later.
/// </summary>
public interface IPassengerDemandProvider
{
    IEnumerable<Station> GetDestinations(
        Station origin,
        IReadOnlyList<Station> availableStations,
        int requestedPassengers);
}
