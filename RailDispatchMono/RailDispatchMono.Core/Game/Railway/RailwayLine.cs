using System;
using System.Collections.Generic;
using System.Linq;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Player-defined logical grouping of track cells. It does not replace blocks or route topology.</summary>
public sealed class RailwayLine
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public int ColorIndex { get; private set; }
    public HashSet<MapPosition> TrackPositions { get; } = new();

    public RailwayLine(string name, Guid? id = null, int colorIndex = 0)
    {
        Id = id ?? Guid.NewGuid();
        Name = string.IsNullOrWhiteSpace(name) ? "Nowy szlak" : name.Trim();
        ColorIndex = Math.Max(0, colorIndex);
    }

    public void Rename(string name)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name.Trim();
    }

    public void SetColorIndex(int colorIndex) => ColorIndex = Math.Max(0, colorIndex);
    public bool Contains(MapPosition position) => TrackPositions.Contains(position);
    public void Add(MapPosition position) => TrackPositions.Add(position);
    public void Remove(MapPosition position) => TrackPositions.Remove(position);
}

/// <summary>Owns player-created railway-line groupings and bulk infrastructure operations.</summary>
public sealed class RailwayLineManager
{
    public static RailwayLineManager? Current { get; private set; }
    private readonly GameMap _map;
    private readonly List<RailwayLine> _lines = new();

    public IReadOnlyList<RailwayLine> Lines => _lines;

    public RailwayLineManager(GameMap map)
    {
        _map = map;
        Current = this;
    }

    public RailwayLine Create(string name, IEnumerable<MapPosition> positions)
    {
        var line = new RailwayLine(name, colorIndex: _lines.Count);
        foreach (var position in positions.Distinct())
            if (_map.HasTrack(position)) line.Add(position);
        _lines.Add(line);
        return line;
    }

    public RailwayLine Create(string name, IEnumerable<MapPosition> positions, Guid id, int colorIndex)
    {
        var line = new RailwayLine(name, id, colorIndex);
        foreach (var position in positions.Distinct())
            if (_map.HasTrack(position)) line.Add(position);
        _lines.Add(line);
        return line;
    }

    public bool Delete(Guid id)
    {
        int index = _lines.FindIndex(x => x.Id == id);
        if (index < 0) return false;
        _lines.RemoveAt(index);
        return true;
    }

    public RailwayLine? Find(Guid id) => _lines.FirstOrDefault(x => x.Id == id);

    public RailwayLine? FindContaining(MapPosition position) => _lines.FirstOrDefault(x => x.Contains(position));

    public int ApplyInfrastructure(RailwayLine line, TrackType type, LineClass lineClass, TractionSystem traction)
    {
        int count = 0;
        foreach (var position in line.TrackPositions)
        {
            var track = _map.GetTrackAt(position);
            if (track == null) continue;
            track.SetInfrastructure(type, lineClass, traction);
            count++;
        }
        return count;
    }

    public int ApplyInfrastructure(IEnumerable<MapPosition> positions, TrackType type, LineClass lineClass, TractionSystem traction)
    {
        int count = 0;
        foreach (var position in positions.Distinct())
        {
            var track = _map.GetTrackAt(position);
            if (track == null) continue;
            track.SetInfrastructure(type, lineClass, traction);
            count++;
        }
        return count;
    }

    public void AddPositions(RailwayLine line, IEnumerable<MapPosition> positions)
    {
        foreach (var position in positions.Distinct())
            if (_map.HasTrack(position)) line.Add(position);
    }

    public void RemovePositions(RailwayLine line, IEnumerable<MapPosition> positions)
    {
        foreach (var position in positions.Distinct()) line.Remove(position);
    }

    public void RemovePosition(MapPosition position)
    {
        foreach (var line in _lines) line.Remove(position);
    }

    public void Clear() => _lines.Clear();

    public static IReadOnlyList<MapPosition> CollectConnectedTracks(GameMap map, MapPosition start)
    {
        if (!map.TryGetTrack(start, out var first) || first == null) return Array.Empty<MapPosition>();
        var result = new List<MapPosition>();
        var visited = new HashSet<MapPosition>();
        var queue = new Queue<MapPosition>();
        queue.Enqueue(start);
        visited.Add(start);
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            result.Add(position);
            var track = map.GetTrackAt(position);
            if (track == null) continue;
            foreach (var direction in track.GetAvailableDirections())
            {
                var next = Next(position, direction);
                if (visited.Contains(next) || !map.HasTrack(next)) continue;
                visited.Add(next);
                queue.Enqueue(next);
            }
        }
        return result;
    }

    private static MapPosition Next(MapPosition position, TrackConnections direction) => direction switch
    {
        TrackConnections.North => new MapPosition(position.X, position.Y - 1),
        TrackConnections.East => new MapPosition(position.X + 1, position.Y),
        TrackConnections.South => new MapPosition(position.X, position.Y + 1),
        TrackConnections.West => new MapPosition(position.X - 1, position.Y),
        _ => position
    };
}
