using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.RollingStock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainManager
{
    private readonly List<Train> _trains = new();
    private readonly GameMap _map;

    public IReadOnlyList<Train> Trains => _trains;
    public GameMap Map => _map;

    public TrainManager(GameMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public void Add(Train train)
    {
        if (train == null) throw new ArgumentNullException(nameof(train));
        if (!_trains.Contains(train))
            _trains.Add(train);
    }

    public bool Remove(Train train) => train != null && _trains.Remove(train);

    public void Clear() => _trains.Clear();

    public bool IsCellOccupied(MapPosition position)
    {
        foreach (var train in _trains)
        {
            if (train.GetCurrentCell() == position)
                return true;

            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                if (new MapPosition(
                    (int)MathF.Floor(train.GetVehicleTransform(i).Position.X),
                    (int)MathF.Floor(train.GetVehicleTransform(i).Position.Y)) == position)
                    return true;
            }
        }

        return false;
    }

    public Train CreateTrainFromComposition(
        TrainComposition composition,
        MapPosition startPosition,
        TrackConnections direction = TrackConnections.East,
        float speed = 0f)
    {
        if (composition == null) throw new ArgumentNullException(nameof(composition));
        if (!composition.CanMove) throw new InvalidOperationException("A train composition must contain a locomotive.");

        // A depot stores the composition in the UI order [L][W][W], while a
        // spawned train needs the locomotive at the rear of the physical consist
        // relative to the depot exit: [W][W][L]. Do not mutate the depot
        // composition; create the runtime train from a reversed vehicle list.
        var vehicles = composition.Vehicles.Reverse().ToList();
        TrackConnections initialDirection = direction == TrackConnections.None
            ? TrackConnections.East
            : direction;

        // startPosition is the locomotive/head spawn point used by the generic
        // train creation path. With the reversed depot list, the last physical
        // vehicle in the consist is the locomotive and the wagons trail behind it.
        Vector2 spawn = new(startPosition.X + 0.5f, startPosition.Y + 0.5f);

        var train = new Train(spawn, initialDirection, speed, vehicles);
        train.SetMap(_map);
        Add(train);
        return train;
    }
}
