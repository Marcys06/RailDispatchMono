using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>
/// Minimal first-generation train collision protection.
/// A train is protected by its next matching signal when that signal is reached
/// before the other train on the currently selected track path. Without such a
/// signal, another train inside the three-cell safety distance causes RadioStop.
/// </summary>
public sealed class TrainCollisionController
{
    private const float SafetyDistanceCells = 3f;
    private readonly GameMap _map;
    private readonly TrainManager _trains;

    public TrainCollisionController(GameMap map, TrainManager trains)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _trains = trains ?? throw new ArgumentNullException(nameof(trains));
    }

    public bool ShouldRadioStop(Train train)
    {
        if (train == null || train.Composition.Vehicles.Count == 0)
            return false;

        Signal? protectingSignal = train.GetNextSignal();
        MapPosition current = train.GetCurrentCell();
        TrackConnections direction = train.Direction;
        float travelled = train.GetDistanceToBoundary();
        var visited = new HashSet<MapPosition>();

        if (protectingSignal?.Position == current)
            return false;
        if (ContainsAnotherTrain(current, train))
            return true;

        for (int step = 0; step < 5; step++)
        {
            if (!visited.Add(current))
                return false;

            if (!_map.TryGetTrack(current, out TrackCell? track) || track == null)
                return false;

            MapPosition next = NextCell(current, direction);
            if (!_map.TryGetTrack(next, out TrackCell? nextTrack) || nextTrack == null)
                return false;

            TrackConnections entrySide = Opposite(direction);
            if (!nextTrack.HasConnection(entrySide))
                return false;

            TrackConnections exit = nextTrack.GetExitDirection(entrySide);
            if (exit == TrackConnections.None)
                return false;

            travelled += 1f;
            current = next;
            direction = exit;

            if (protectingSignal?.Position == current)
                return false;

            if (travelled <= SafetyDistanceCells && ContainsAnotherTrain(current, train))
                return true;

            if (travelled > SafetyDistanceCells)
                return false;
        }

        return false;
    }

    private bool ContainsAnotherTrain(MapPosition cell, Train source)
    {
        foreach (var train in _trains.Trains)
        {
            if (train.Id == source.Id)
                continue;

            foreach (var position in train.GetVehiclePositions())
            {
                var vehicleCell = new MapPosition(
                    (int)MathF.Floor(position.X),
                    (int)MathF.Floor(position.Y));
                if (vehicleCell == cell)
                    return true;
            }
        }
        return false;
    }

    private static MapPosition NextCell(MapPosition position, TrackConnections direction) =>
        direction switch
        {
            TrackConnections.North => new MapPosition(position.X, position.Y - 1),
            TrackConnections.East => new MapPosition(position.X + 1, position.Y),
            TrackConnections.South => new MapPosition(position.X, position.Y + 1),
            TrackConnections.West => new MapPosition(position.X - 1, position.Y),
            _ => position
        };

    private static TrackConnections Opposite(TrackConnections direction) =>
        direction switch
        {
            TrackConnections.North => TrackConnections.South,
            TrackConnections.East => TrackConnections.West,
            TrackConnections.South => TrackConnections.North,
            TrackConnections.West => TrackConnections.East,
            _ => TrackConnections.None
        };
}
