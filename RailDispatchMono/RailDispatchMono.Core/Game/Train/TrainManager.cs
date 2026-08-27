using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainManager
{
    private readonly List<Train> _trains = new();

    public IReadOnlyList<Train> Trains =>
        _trains;

    public void Add(
        Train train)
    {
        if (!_trains.Contains(train))
        {
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
