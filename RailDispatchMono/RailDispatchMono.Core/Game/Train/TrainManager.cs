
using System.Collections.Generic;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainManager
{
    private readonly GameMap _map;
    private readonly List<Train> _trains = new();

    public IReadOnlyList<Train> Trains =>
        _trains;

    public TrainManager(
        GameMap map)
    {
        _map = map;
    }

    public void Add(
        Train train)
    {
        if (!_trains.Contains(train))
        {
            train.SetMap(_map);
            _trains.Add(train);
        }
    }

    public bool Remove(
        Train train)
    {
        return _trains.Remove(train);
    }

    public void Update(
        float deltaTime)
    {
        foreach (var train in _trains)
        {
            train.Update(deltaTime);
        }
    }
}

