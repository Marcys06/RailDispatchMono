using RailDispatchMono.Core.Game.Map;
using System;

namespace RailDispatchMono.Core.Game.Railway;

public sealed class Depot
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; }
    public MapPosition Position { get; }

    public Depot(string name, MapPosition position)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Depot" : name;
        Position = position;
    }

    public Depot(Guid id, string name, MapPosition position)
    {
        Id = id;
        Name = string.IsNullOrWhiteSpace(name) ? "Depot" : name;
        Position = position;
    }
}