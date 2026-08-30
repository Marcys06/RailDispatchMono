namespace RailDispatchMono.Core.Game.Train;

public class VehicleParameters
{
    public float MaxSpeed { get; }
    public float Acceleration { get; }
    public float Braking { get; }
    public float Mass { get; }
    public float Length { get; }

    /// <summary>Mass influence coefficient x used by the train physics model.</summary>
    public float MassCoefficient { get; }

    /// <summary>Technical condition multiplier k. 1.0 means nominal condition.</summary>
    public float TechnicalCondition { get; }

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
        Acceleration = acceleration;
        Braking = braking;
        Mass = mass;
        Length = length;
        MassCoefficient = massCoefficient > 0f ? massCoefficient : 0.01f;
        TechnicalCondition = MathF.Max(0.5f, MathF.Min(1.5f, technicalCondition));
    }
}
