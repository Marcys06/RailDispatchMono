using RailDispatchMono.Core.Game.Map;
using System;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// Lightweight depot model. It intentionally stores a map position and a display
/// name only; train creation remains a Gameplay/UI concern for now. The model is
/// ready to become a route origin in 0.0.13.
/// </summary>
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
}
