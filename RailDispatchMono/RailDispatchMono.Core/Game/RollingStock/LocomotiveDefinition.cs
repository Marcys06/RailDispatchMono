using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.RollingStock;

public sealed class LocomotiveDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public TractionType Traction { get; }
    public IReadOnlySet<TractionSystem> SupportedTractionSystems { get; }
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
        string? texturePath = null,
        params TractionSystem[] supportedTractionSystems)
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
        SupportedTractionSystems = new HashSet<TractionSystem>(supportedTractionSystems);
    }

    public bool CanOperateOn(TractionSystem system)
        => Traction == TractionType.Diesel ||
           system == TractionSystem.None ? Traction == TractionType.Diesel : SupportedTractionSystems.Contains(system);

    public Locomotive CreateVehicle()
    {
        var parameters = VehicleParameters.CreatePhysical(
            MaxSpeedKmh,
            AccelerationMps2,
            DecelerationMps2,
            MassTons,
            LengthMeters,
            1.0f,
            MassCoefficient);

        return new Locomotive(LocomotiveType, parameters, Id, Traction, SupportedTractionSystems);
    }
}
