using RailDispatchMono.Core.Game.Train;

namespace RailDispatchMono.Core.Game.RollingStock;

public sealed class LocomotiveDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public TractionType Traction { get; }
    public LocomotiveType LocomotiveType { get; }
    public float MaxSpeedKmh { get; }
    public float MassTons { get; }
    public float LengthMeters { get; }
    public float AccelerationMps2 { get; }
    public float DecelerationMps2 { get; }
    public float MassCoefficient { get; }
    public string? TexturePath { get; }

    public LocomotiveDefinition(
        string id,
        string displayName,
        TractionType traction,
        LocomotiveType locomotiveType,
        float maxSpeedKmh,
        float massTons,
        float lengthMeters,
        float accelerationMps2,
        float decelerationMps2,
        float massCoefficient = 0.01f,
        string? texturePath = null)
    {
        Id = id;
        DisplayName = displayName;
        Traction = traction;
        LocomotiveType = locomotiveType;
        MaxSpeedKmh = maxSpeedKmh;
        MassTons = massTons;
        LengthMeters = lengthMeters;
        AccelerationMps2 = accelerationMps2;
        DecelerationMps2 = decelerationMps2;
        MassCoefficient = massCoefficient;
        TexturePath = texturePath;
    }

    public Locomotive CreateVehicle()
    {
        var parameters = VehicleParameters.CreatePhysical(
            MaxSpeedKmh,
            AccelerationMps2,
            DecelerationMps2,
            MassTons,
            LengthMeters,
            visualLengthCells: 1.0f,
            MassCoefficient);

        return new Locomotive(LocomotiveType, parameters);
    }
}
