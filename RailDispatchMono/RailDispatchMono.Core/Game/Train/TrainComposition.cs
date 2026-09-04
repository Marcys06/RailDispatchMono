using RailDispatchMono.Core.Game.RollingStock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainComposition
{
    private readonly List<Vehicle> _vehicles = new();

    public IReadOnlyList<Vehicle> Vehicles => _vehicles;

    /// <summary>
    /// Rebuilds the explicit vehicle order metadata to match the current
    /// physical order of the consist. Travel direction must not change it.
    /// </summary>
    public void NormalizeCompositionOrder()
    {
        for (int i = 0; i < _vehicles.Count; i++)
            _vehicles[i].CompositionOrder = i;
    }

    public float Length
    {
        get
        {
            float length = 0f;
            foreach (var vehicle in _vehicles)
                length += vehicle.Parameters.Length;
            return length;
        }
    }

    public float TotalLengthMeters => _vehicles.Sum(v => v.Parameters.LengthMeters);
    public float TotalMass => _vehicles.Sum(v => v.Parameters.MassTons);
    public float EffectiveMaxSpeed => _vehicles.Count == 0 ? 0f : _vehicles.Min(v => v.Parameters.MaxSpeed);
    public float EffectiveMaxSpeedKmh => EffectiveMaxSpeed * 3.6f;
    public int WagonCount => _vehicles.Count(v => v is Wagon);
    public Locomotive? Locomotive => _vehicles.OfType<Locomotive>().FirstOrDefault();

    public bool CanMove
    {
        get
        {
            foreach (var vehicle in _vehicles)
                if (vehicle is Locomotive)
                    return true;
            return false;
        }
    }

    public void AddVehicle(Vehicle vehicle)
    {
        if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));

        if (_vehicles.Count > 0)
        {
            var previous = _vehicles[^1];
            InitializeAdjacentCoupling(previous, vehicle);
        }

        _vehicles.Add(vehicle);
        vehicle.CompositionOrder = _vehicles.Count - 1;
    }

    private static void InitializeAdjacentCoupling(Vehicle previous, Vehicle next)
    {
        if (previous.CouplingState.IsOccupied(VehicleEnd.Rear) ||
            next.CouplingState.IsOccupied(VehicleEnd.Front))
            return;

        var previousCoupler = previous.Coupling.Get(VehicleEnd.Rear);
        var nextCoupler = next.Coupling.Get(VehicleEnd.Front);
        if (previousCoupler == CouplerType.None ||
            nextCoupler == CouplerType.None ||
            previousCoupler != nextCoupler)
            return;

        var connection = new CouplingConnection(
            previous,
            VehicleEnd.Rear,
            next,
            VehicleEnd.Front);

        previous.CouplingState.Set(VehicleEnd.Rear, connection);
        next.CouplingState.Set(VehicleEnd.Front, connection);
    }

    public void RebuildRuntimeCouplings()
    {
        foreach (var vehicle in _vehicles)
        {
            vehicle.CouplingState.Set(VehicleEnd.Front, null);
            vehicle.CouplingState.Set(VehicleEnd.Rear, null);
        }

        for (int i = 1; i < _vehicles.Count; i++)
            InitializeAdjacentCoupling(_vehicles[i - 1], _vehicles[i]);

        NormalizeCompositionOrder();
    }

    public void SetLocomotive(LocomotiveDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        int locomotiveIndex = _vehicles.FindIndex(v => v is Locomotive);
        var locomotive = definition.CreateVehicle();
        if (locomotiveIndex >= 0)
            _vehicles[locomotiveIndex] = locomotive;
        else
            _vehicles.Insert(0, locomotive);

        RebuildRuntimeCouplings();
    }

    public void AddWagon(WagonDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        AddVehicle(definition.CreateVehicle());
    }

    public bool RemoveVehicle(Vehicle vehicle)
    {
        bool removed = _vehicles.Remove(vehicle);
        if (removed)
            NormalizeCompositionOrder();
        return removed;
    }

    public bool RemoveWagon(int index)
    {
        if (index < 0 || index >= _vehicles.Count || _vehicles[index] is not Wagon)
            return false;
        _vehicles.RemoveAt(index);
        NormalizeCompositionOrder();
        return true;
    }

    public void InsertVehicle(int index, Vehicle vehicle)
    {
        _vehicles.Insert(index, vehicle);
        NormalizeCompositionOrder();
    }

    public TrainComposition Split(int index)
    {
        if (index <= 0 || index >= _vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var splitComposition = new TrainComposition();
        while (_vehicles.Count > index)
        {
            var vehicle = _vehicles[index];
            _vehicles.RemoveAt(index);
            splitComposition.AddVehicle(vehicle);
        }

        NormalizeCompositionOrder();
        return splitComposition;
    }

    public void Clear() => _vehicles.Clear();
}
