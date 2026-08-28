using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainManager
{
    private readonly GameMap _map;
    private readonly List<Train> _trains = new();
    private readonly List<Train> _trainsToAdd = new();
    private readonly List<Train> _trainsToRemove = new();

    public IReadOnlyList<Train> Trains => _trains;

    public TrainManager(GameMap map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public void Add(Train train)
    {
        if (train == null)
            throw new ArgumentNullException(nameof(train));

        if (!_trains.Contains(train) && !_trainsToAdd.Contains(train))
        {
            train.SetMap(_map);
            _trainsToAdd.Add(train);
        }
    }

    public Train CreateTrain(Vector2 position, TrackConnections direction, float speed)
    {
        var train = new Train(position, direction, speed);
        train.SetMap(_map);
        _trainsToAdd.Add(train);
        return train;
    }

    public Train CreateTrain(MapPosition cell, TrackConnections direction, float speed)
    {
        Vector2 position = new Vector2(cell.X + 0.5f, cell.Y + 0.5f);
        return CreateTrain(position, direction, speed);
    }

    public bool Remove(Train train)
    {
        if (train == null)
            return false;

        if (_trains.Contains(train))
        {
            _trainsToRemove.Add(train);
            return true;
        }

        return false;
    }

    public void ClearAll()
    {
        foreach (var train in _trains)
        {
            if (!_trainsToRemove.Contains(train))
                _trainsToRemove.Add(train);
        }
    }

    public void Update(float deltaTime)
    {
        // Dodaj nowe pociągi
        if (_trainsToAdd.Count > 0)
        {
            foreach (var train in _trainsToAdd)
            {
                if (!_trains.Contains(train))
                {
                    train.SetMap(_map);
                    _trains.Add(train);
                }
            }
            _trainsToAdd.Clear();
        }

        // Usuń pociągi oznaczone do usunięcia
        if (_trainsToRemove.Count > 0)
        {
            foreach (var train in _trainsToRemove)
            {
                _trains.Remove(train);
            }
            _trainsToRemove.Clear();
        }

        // Aktualizuj każdy pociąg
        foreach (var train in _trains)
        {
            train.Update(deltaTime);
        }
    }

    public bool IsCellOccupied(MapPosition cell)
    {
        foreach (var train in _trains)
        {
            if (train.GetCurrentCell() == cell)
                return true;
        }
        return false;
    }

    public Train? GetTrainAtCell(MapPosition cell)
    {
        foreach (var train in _trains)
        {
            if (train.GetCurrentCell() == cell)
                return train;
        }
        return null;
    }

    public List<Train> GetTrainsInRadius(Vector2 center, float radius)
    {
        var result = new List<Train>();
        float radiusSquared = radius * radius;

        foreach (var train in _trains)
        {
            float distanceSquared = Vector2.DistanceSquared(center, train.Position);
            if (distanceSquared <= radiusSquared)
                result.Add(train);
        }

        return result;
    }

    public int Count => _trains.Count;
    public bool HasAnyTrains => _trains.Count > 0;

    public List<Vector2> GetAllHeadPositions()
    {
        var positions = new List<Vector2>(_trains.Count);
        foreach (var train in _trains)
        {
            positions.Add(train.Position);
        }
        return positions;
    }

    public Dictionary<Train, Vector2[]> GetAllVehiclePositions(float vehicleSpacing = 1.0f)
    {
        var result = new Dictionary<Train, Vector2[]>();

        foreach (var train in _trains)
        {
            var positions = train.GetVehiclePositions(vehicleSpacing);
            result[train] = positions.ToArray();
        }

        return result;
    }

    public Dictionary<Train, float> GetAllRotations()
    {
        var result = new Dictionary<Train, float>();

        foreach (var train in _trains)
        {
            result[train] = train.GetRotation();
        }

        return result;
    }

    public Dictionary<Train, (Vector2 Position, float Rotation)[]> GetAllVehicleTransforms()
    {
        var result = new Dictionary<Train, (Vector2 Position, float Rotation)[]>();

        foreach (var train in _trains)
        {
            var transforms = new (Vector2, float)[train.Composition.Vehicles.Count];
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                transforms[i] = train.GetVehicleTransform(i);
            }
            result[train] = transforms;
        }

        return result;
    }
}