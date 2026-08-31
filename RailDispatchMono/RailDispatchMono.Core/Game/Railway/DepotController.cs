using RailDispatchMono.Core.Game.Map;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Owns player-built depot buildings.</summary>
public sealed class DepotController
{
    private readonly List<Depot> _depots = new();

    public IReadOnlyList<Depot> Depots => _depots;

    public bool AddDepot(Depot depot)
    {
        if (depot == null) throw new ArgumentNullException(nameof(depot));
        if (_depots.Any(d => d.Id == depot.Id)) return false;
        if (GetDepotAt(depot.Position) != null) return false;
        _depots.Add(depot);
        return true;
    }

    public bool RemoveDepot(Depot depot) => depot != null && _depots.Remove(depot);

    public Depot? GetDepotAt(MapPosition position) =>
        _depots.FirstOrDefault(d => d.Position == position);
}
