namespace RailDispatchMono.Core.Game.Train;

using RailDispatchMono.Core.Game.Simulation;
using System;

public class VehicleParameters
{
    public float MaxSpeed { get; }
    public float Acceleration { get; }
    public float Braking { get; }
    public float Mass { get; }
    public float Length { get; }

    /// <summary>Real-world mass in tonnes. Legacy vehicle values are derived from kilograms.</summary>
    public float MassTons { get; }

    /// <summary>Real-world length in metres. Legacy vehicle values are derived from map cells.</summary>
    public float LengthMeters { get; }

    public float MassCoefficient { get; }
    public float TechnicalCondition { get; }
    public float AccelerationCoefficient { get; }
    public float BrakingCoefficient { get; }

    private const float AccelerationResponseScale = 0.5f;
    private const float BrakingResponseScale = 0.35f;

    public VehicleParameters(
        float maxSpeed,
        float acceleration,
        float braking,
        float mass,
        float length,
        float massCoefficient = 0.01f,
        float technicalCondition = 1.0f)
    {
        MaxSpeed = maxSpeed;
        Mass = Math.Max(0.001f, mass);
        Length = Math.Max(0f, length);
        MassTons = Mass / 1000f;
        LengthMeters = SimulationScale.GridToMeters(Length);

        MassCoefficient = Math.Max(0.000001f, massCoefficient);
        TechnicalCondition = Math.Max(0.5f, Math.Min(1.5f, technicalCondition));

        AccelerationCoefficient = Math.Max(0f, acceleration);
        BrakingCoefficient = Math.Max(0f, (braking > 10f ? braking / 100f : braking) * 20.0f);

        Acceleration = CalculateRate(AccelerationCoefficient) * AccelerationResponseScale;
        Braking = CalculateRate(BrakingCoefficient) * BrakingResponseScale;
    }

    private VehicleParameters(
        float maxSpeed,
        float acceleration,
        float braking,
        float massTons,
        float lengthMeters,
        float visualLengthCells,
        float massCoefficient,
        float technicalCondition)
    {
        MaxSpeed = Math.Max(0f, maxSpeed);
        Acceleration = Math.Max(0f, acceleration);
        Braking = Math.Max(0f, braking);
        MassTons = Math.Max(0.001f, massTons);
        Mass = MassTons * 1000f;
        LengthMeters = Math.Max(0f, lengthMeters);
        Length = Math.Max(0f, visualLengthCells);
        MassCoefficient = Math.Max(0.000001f, massCoefficient);
        TechnicalCondition = Math.Max(0.5f, Math.Min(1.5f, technicalCondition));
        AccelerationCoefficient = Acceleration;
        BrakingCoefficient = Braking;
    }

    /// <summary>
    /// Creates a rolling-stock parameter set from physical gameplay values.
    /// Internal speed remains m/s and is converted from the catalog's km/h.
    /// Physical length/mass are stored separately so the 10 m map-cell scale
    /// does not shrink the established visual vehicle proportions.
    /// </summary>
    public static VehicleParameters CreatePhysical(
        float maxSpeedKmh,
        float accelerationMps2,
        float decelerationMps2,
        float massTons,
        float lengthMeters,
        float visualLengthCells = 1.0f,
        float massCoefficient = 0.01f,
        float technicalCondition = 1.0f)
    {
        return new VehicleParameters(
            maxSpeedKmh / 3.6f,
            accelerationMps2,
            decelerationMps2,
            massTons,
            lengthMeters,
            visualLengthCells,
            massCoefficient,
            technicalCondition);
    }

    private float CalculateRate(float d)
    {
        if (d <= 0f)
            return 0f;

        double exponent = d * Math.Pow(MassCoefficient, 1d);
        double rate = Math.Pow(Mass, exponent) * TechnicalCondition;

        if (double.IsNaN(rate) || double.IsInfinity(rate))
            return 0f;

        return (float)Math.Clamp(rate, 0d, 5000d);
    }
}
