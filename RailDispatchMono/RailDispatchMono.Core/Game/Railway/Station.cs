using RailDispatchMono.Core.Game.Map;
using System;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// A passenger station anchored to a map cell. The first implementation is
/// intentionally route-agnostic: every station is a valid origin/destination
/// and passenger trains stop there when the station controller requests service.
/// </summary>
public sealed class Station
{
    public Guid Id { get; }
    public string Name { get; set; }
    public MapPosition Position { get; }
    public float StopRadius { get; set; } = 0.35f;
    public float DwellTimeSeconds { get; set; } = 5f;
    public bool PassengerServiceEnabled { get; set; } = true;

    public Station(string name, MapPosition position)
    {
        Id = Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? $"Station-{Id.ToString()[..8]}" : name;
        Position = position;
    }
}
