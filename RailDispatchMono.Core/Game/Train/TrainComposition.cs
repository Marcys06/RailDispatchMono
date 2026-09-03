using RailDispatchMono.Core.Game.RollingStock;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed class TrainComposition
{
    private const float PowerToMassThresholdMWPerTon = 0.006f;
    private const float PowerLoadExponent = 0.55f;

    private readonly List<Vehicle> _vehicles = new();

    public IReadOnlyList<Vehicle> Vehicles => _vehicles;

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
    public float BaseMaxSpeed => _vehicles.Count == 0 ? 0f : _vehicles.Min(v => v.Parameters.MaxSpeed);
    public float EffectiveMaxSpeed => GetEffectiveMaxSpeed();
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

    public float GetEffectiveMaxSpeed()
    {
        if (_vehicles.Count == 0)
            return 0f;

        var locomotive = Locomotive;
        if (locomotive is null)
            return BaseMaxSpeed;

        float locomotiveBaseSpeed = locomotive.Parameters.MaxSpeed;
        float wagonMaxSpeed = _vehicles
            .Where(v => v is Wagon)
            .Select(v => v.Parameters.MaxSpeed)
            .DefaultIfEmpty(float.MaxValue)
            .Min();

        float loadMultiplier = GetPowerLoadMultiplier(locomotive);
        return MathF.Min(wagonMaxSpeed, locomotiveBaseSpeed * loadMultiplier);
    }

    private float GetPowerLoadMultiplier(Locomotive locomotive)
    {
        if (locomotive.Parameters is not LocomotiveParameters parameters || parameters.PowerMW <= 0f)
            return 0f;

        float supportedMass = parameters.PowerMW / PowerToMassThresholdMWPerTon;
        float totalMass = MathF.Max(parameters.MassTons, TotalMass);
        if (totalMass <= supportedMass)
            return 1f;

        float loadRatio = supportedMass / totalMass;
        return MathF.Pow(MathF.Max(0f, loadRatio), PowerLoadExponent);
    }

    public void AddVehicle(Vehicle vehicle)
    {
        if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
        _vehicles.Add(vehicle);
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
    }

    public void AddWagon(WagonDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        _vehicles.Add(definition.CreateVehicle());
    }

    public bool RemoveVehicle(Vehicle vehicle) => _vehicles.Remove(vehicle);

    public bool RemoveWagon(int index)
    {
        if (index < 0 || index >= _vehicles.Count || _vehicles[index] is not Wagon)
            return false;
        _vehicles.RemoveAt(index);
        return true;
    }

    public void InsertVehicle(int index, Vehicle vehicle) => _vehicles.Insert(index, vehicle);

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
        return splitComposition;
    }

    public void Clear() => _vehicles.Clear();
}
