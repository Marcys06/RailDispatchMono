using System;
using System.Collections.Generic;
using System.Linq;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Player-defined logical grouping of track cells. It does not replace blocks or route topology.</summary>
public sealed class RailwayLine
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; private set; }
    public HashSet<MapPosition> TrackPositions { get; } = new();

    public RailwayLine(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Nowy szlak" : name.Trim();
    }

    public void Rename(string name)
    {
        if (!string.IsNullOrWhiteSpace(name)) Name = name.Trim();
    }

    public bool Contains(MapPosition position) => TrackPositions.Contains(position);
    public void Add(MapPosition position) => TrackPositions.Add(position);
    public void Remove(MapPosition position) => TrackPositions.Remove(position);
}

/// <summary>Owns player-created railway-line groupings and provides bulk infrastructure operations.</summary>
public sealed class RailwayLineManager
{
    private readonly List<RailwayLine> _lines = new();
    public IReadOnlyList<RailwayLine> Lines => _lines;

    public RailwayLine Create(string name, IEnumerable<MapPosition> positions)
    {
        var line = new RailwayLine(name);
        foreach (var position in positions.Distinct()) line.Add(position);
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

    public int Assign(RailwayLine line, IEnumerable<MapPosition> positions)
    {
        int count = 0;
        foreach (var position in positions.Distinct())
            if (line.TrackPositions.Add(position)) count++;
        return count;
    }

    public int Remove(RailwayLine line, IEnumerable<MapPosition> positions)
    {
        int count = 0;
        foreach (var position in positions.Distinct())
            if (line.TrackPositions.Remove(position)) count++;
        return count;
    }

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
