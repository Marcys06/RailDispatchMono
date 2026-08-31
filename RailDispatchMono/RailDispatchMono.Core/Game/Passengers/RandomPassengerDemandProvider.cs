using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Passengers;

/// <summary>
/// Temporary demand model. It chooses random destinations uniformly from all
/// other stations. It is intentionally isolated so a future city model can replace it.
/// </summary>
public sealed class RandomPassengerDemandProvider : IPassengerDemandProvider
{
    private readonly Random _random;

    public RandomPassengerDemandProvider(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public IEnumerable<Station> GetDestinations(
        Station origin,
        IReadOnlyList<Station> availableStations,
        int requestedPassengers)
    {
        var candidates = availableStations.Where(s => s.Id != origin.Id).ToList();
        if (candidates.Count == 0 || requestedPassengers <= 0)
            yield break;

        for (int i = 0; i < requestedPassengers; i++)
            yield return candidates[_random.Next(candidates.Count)];
    }
}
