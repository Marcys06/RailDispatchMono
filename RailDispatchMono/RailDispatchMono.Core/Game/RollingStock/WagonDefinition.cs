using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.RollingStock;

public sealed class WagonDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public float MassTons { get; }
    public float LengthMeters { get; }
    public float MaxSpeedKmh { get; }
    public WagonType Type { get; }
    public int Capacity { get; }

    public WagonDefinition(
        string id,
        string displayName,
        float massTons,
        float lengthMeters,
        float maxSpeedKmh,
        WagonType type,
        int capacity)
    {
        Id = id;
        DisplayName = displayName;
        MassTons = massTons;
        LengthMeters = lengthMeters;
        MaxSpeedKmh = maxSpeedKmh;
        Type = type;
        Capacity = capacity;
    }

    public Wagon CreateVehicle()
    {
        var parameters = VehicleParameters.CreatePhysical(
            MaxSpeedKmh,
            accelerationMps2: 0f,
            decelerationMps2: 0f,
            MassTons,
            LengthMeters,
            visualLengthCells: 1.0f);

        return new Wagon(parameters, Type, Capacity);
    }
}
