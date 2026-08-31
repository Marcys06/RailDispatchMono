using RailDispatchMono.Core.Game.Map;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>
/// Passenger station represented by an area of map cells.
/// The Position property is retained as the anchor/top-left cell for compatibility.
/// Semaphores remain responsible for defining where a train stops; station geometry
/// only defines which cells belong to the station and therefore where passenger
/// service is available.
/// </summary>
public sealed class Station
{
    public Guid Id { get; }
    public string Name { get; set; }

    /// <summary>Anchor (top-left) cell of the station area.</summary>
    public MapPosition Position { get; }

    public int Width { get; }
    public int Height { get; }

    public float StopRadius { get; set; } = 0.35f;
    public float DwellTimeSeconds { get; set; } = 5f;
    public bool PassengerServiceEnabled { get; set; } = true;

    public int AreaSize => Width * Height;

    public Station(string name, MapPosition position, int width = 1, int height = 1)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Id = Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? $"Station-{Id.ToString()[..8]}" : name;
        Position = position;
        Width = width;
        Height = height;
    }

    public bool Contains(MapPosition position) =>
        position.X >= Position.X && position.X < Position.X + Width &&
        position.Y >= Position.Y && position.Y < Position.Y + Height;

    public IEnumerable<MapPosition> GetCells()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                yield return new MapPosition(Position.X + x, Position.Y + y);
    }

    public MapPosition GetCenterCell() =>
        new(Position.X + Width / 2, Position.Y + Height / 2);
}
