using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Building;

public sealed class TrackBuilder
{
    private readonly GameMap _map;

    public TrackBuildMode Mode { get; set; } = TrackBuildMode.Straight;

    public CurveDirection Curve { get; set; } = CurveDirection.NorthEast;

    public JunctionType Junction { get; set; } = JunctionType.South_NorthEast;

    public bool StraightHorizontal { get; set; } = true;

    public TrackBuilder(GameMap map)
    {
        _map = map;
    }

    /// <summary>
    /// Główna metoda stawiająca tor/zwrotnicę zależnie od aktualnie wybranego Mode.
    /// </summary>
    public void BuildAt(MapPosition position)
    {
        switch (Mode)
        {
            case TrackBuildMode.Straight:
                BuildStraight(position, StraightHorizontal);
                break;

            case TrackBuildMode.Curve:
                BuildCurve(position, Curve);
                break;

            case TrackBuildMode.Junction:
                BuildJunctionFromType(position, Junction);
                break;
        }
    }

    public void BuildStraight(MapPosition position, bool horizontal)
    {
        if (!IsInsideMap(position))
            return;

        var connections = horizontal
            ? TrackConnections.West | TrackConnections.East
            : TrackConnections.North | TrackConnections.South;

        var track = GetOrCreate(position);

        track.SetGeometry(TrackGeometry.Straight);
        track.SetConnections(connections);

        ConnectNeighbours(position, connections);
    }

    public void BuildJunction(
        MapPosition position,
        TrackConnections stem,
        TrackConnections straight,
        TrackConnections diverging)
    {
        if (!IsInsideMap(position))
            return;

        var track = GetOrCreate(position);
        track.ConfigureJunction(stem, straight, diverging);

        // Połącz sąsiadów dla wszystkich 3 wyjść
        ConnectNeighbours(position, track.Connections);
    }

    public void BuildJunctionFromType(MapPosition position, JunctionType type)
    {
        var (stem, straight, diverging) = type switch
        {
            JunctionType.South_NorthEast => (TrackConnections.South, TrackConnections.North, TrackConnections.East),
            JunctionType.South_NorthWest => (TrackConnections.South, TrackConnections.North, TrackConnections.West),
            JunctionType.West_EastSouth => (TrackConnections.West, TrackConnections.East, TrackConnections.South),
            JunctionType.West_EastNorth => (TrackConnections.West, TrackConnections.East, TrackConnections.North),
            JunctionType.North_SouthEast => (TrackConnections.North, TrackConnections.South, TrackConnections.East),
            JunctionType.North_SouthWest => (TrackConnections.North, TrackConnections.South, TrackConnections.West),
            JunctionType.East_WestSouth => (TrackConnections.East, TrackConnections.West, TrackConnections.South),
            JunctionType.East_WestNorth => (TrackConnections.East, TrackConnections.West, TrackConnections.North),
            _ => (TrackConnections.South, TrackConnections.North, TrackConnections.East)
        };

        BuildJunction(position, stem, straight, diverging);
    }

    public void BuildCurve(MapPosition position, CurveDirection direction)
    {
        if (!IsInsideMap(position))
            return;

        var connections = direction switch
        {
            CurveDirection.NorthEast => TrackConnections.North | TrackConnections.East,
            CurveDirection.EastSouth => TrackConnections.East | TrackConnections.South,
            CurveDirection.SouthWest => TrackConnections.South | TrackConnections.West,
            CurveDirection.WestNorth => TrackConnections.West | TrackConnections.North,
            _ => TrackConnections.None
        };

        var track = GetOrCreate(position);

        track.SetGeometry(TrackGeometry.Curve);
        track.SetConnections(connections);

        ConnectNeighbours(position, connections);
    }

    public void Remove(MapPosition position)
    {
        if (!IsInsideMap(position)) return;

        if (_map.TryGetTrack(position, out var track) && track != null)
        {
            // Zapisz połączenia przed usunięciem
            var connections = track.Connections;

            // Usuń tor
            _map.RemoveTrack(position);

            // Aktualizuj sąsiadów - usuń połączenia do usuniętego toru
            UpdateNeighborsAfterRemoval(position, connections);
        }
    }

    private TrackCell GetOrCreate(MapPosition position)
    {
        if (_map.TryGetTrack(position, out var existing) && existing is not null)
        {
            return existing;
        }

        if (!IsInsideMap(position))
        {
            throw new System.ArgumentOutOfRangeException(nameof(position));
        }

        var track = new TrackCell(
            position,
            TrackGeometry.Straight,
            TrackConnections.None);

        _map.AddTrack(track);

        return track;
    }

    private void ConnectNeighbours(MapPosition position, TrackConnections connections)
    {
        if (connections.HasFlag(TrackConnections.North))
        {
            AddConnection(position.X, position.Y - 1, TrackConnections.South);
        }

        if (connections.HasFlag(TrackConnections.East))
        {
            AddConnection(position.X + 1, position.Y, TrackConnections.West);
        }

        if (connections.HasFlag(TrackConnections.South))
        {
            AddConnection(position.X, position.Y + 1, TrackConnections.North);
        }

        if (connections.HasFlag(TrackConnections.West))
        {
            AddConnection(position.X - 1, position.Y, TrackConnections.East);
        }
    }

    private void AddConnection(int x, int y, TrackConnections connection)
    {
        var position = new MapPosition(x, y);

        if (!IsInsideMap(position))
            return;

        if (!_map.TryGetTrack(position, out var neighbour) || neighbour is null)
        {
            return;
        }

        if (neighbour.Geometry == TrackGeometry.Curve)
        {
            return;
        }

        neighbour.SetConnections(neighbour.Connections | connection);
    }

    private void UpdateNeighborsAfterRemoval(MapPosition position, TrackConnections connections)
    {
        if (connections.HasFlag(TrackConnections.North))
        {
            RemoveConnection(position.X, position.Y - 1, TrackConnections.South);
        }
        if (connections.HasFlag(TrackConnections.East))
        {
            RemoveConnection(position.X + 1, position.Y, TrackConnections.West);
        }
        if (connections.HasFlag(TrackConnections.South))
        {
            RemoveConnection(position.X, position.Y + 1, TrackConnections.North);
        }
        if (connections.HasFlag(TrackConnections.West))
        {
            RemoveConnection(position.X - 1, position.Y, TrackConnections.East);
        }
    }

    private void RemoveConnection(int x, int y, TrackConnections connection)
    {
        var position = new MapPosition(x, y);

        if (!IsInsideMap(position))
            return;

        if (!_map.TryGetTrack(position, out var neighbour) || neighbour is null)
            return;

        // Usuń połączenie
        var updated = neighbour.Connections & ~connection;
        neighbour.SetConnections(updated);

        // Jeśli nie ma już połączeń, usuń tor
        if (updated == TrackConnections.None)
        {
            _map.RemoveTrack(position);
        }
    }

    private bool IsInsideMap(MapPosition position)
    {
        return position.X >= 0 &&
               position.X < _map.Size.Width &&
               position.Y >= 0 &&
               position.Y < _map.Size.Height;
    }
}