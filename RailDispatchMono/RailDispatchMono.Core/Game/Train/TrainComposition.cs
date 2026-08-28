using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainComposition
{
    private readonly List<Vehicle> _vehicles = new();

    public IReadOnlyList<Vehicle> Vehicles =>
        _vehicles;

    public float Length
    {
        get
        {
            float length = 0f;

            foreach (var vehicle in _vehicles)
            {
                length += vehicle.Parameters.Length;
            }

            return length;
        }
    }

    public bool CanMove
    {
        get
        {
            foreach (var vehicle in _vehicles)
            {
                if (vehicle is Locomotive)
                    return true;
            }

            return false;
        }
    }

    public void AddVehicle(
        Vehicle vehicle)
    {
        _vehicles.Add(vehicle);
    }

    public bool RemoveVehicle(
        Vehicle vehicle)
    {
        return _vehicles.Remove(vehicle);
    }

    public void InsertVehicle(
        int index,
        Vehicle vehicle)
    {
        _vehicles.Insert(
            index,
            vehicle);
    }

    public TrainComposition Split(
        int index)
    {
        if (index <= 0 ||
            index >= _vehicles.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index));
        }

        var splitComposition =
            new TrainComposition();

        while (_vehicles.Count > index)
        {
            var vehicle =
                _vehicles[index];

            _vehicles.RemoveAt(index);

            splitComposition.AddVehicle(
                vehicle);
        }

        return splitComposition;
    }
}