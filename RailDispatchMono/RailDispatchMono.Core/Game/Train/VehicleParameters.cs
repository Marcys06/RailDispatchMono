namespace RailDispatchMono.Core.Game.Train;

public class VehicleParameters
{
    public float MaxSpeed { get; }

    /// <summary>Effective acceleration rate calculated from d, m, x and k.</summary>
    public float Acceleration { get; }

    /// <summary>Effective braking rate calculated from d, m, x and k.</summary>
    public float Braking { get; }

    public float Mass { get; }
    public float Length { get; }

    /// <summary>Mass influence coefficient x.</summary>
    public float MassCoefficient { get; }

    /// <summary>Technical condition multiplier k, constrained to &lt;0.5, 1.5&gt;.</summary>
    public float TechnicalCondition { get; }

    /// <summary>Original acceleration coefficient d before the mass/condition model.</summary>
    public float AccelerationCoefficient { get; }

    /// <summary>Original braking coefficient d before the mass/condition model.</summary>
    public float BrakingCoefficient { get; }

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
        Mass = MathF.Max(0.001f, mass);
        Length = length;

        MassCoefficient = MathF.Max(0.000001f, massCoefficient);
        TechnicalCondition = MathF.Max(0.5f, MathF.Min(1.5f, technicalCondition));

        AccelerationCoefficient = MathF.Max(0f, acceleration);
        BrakingCoefficient = MathF.Max(0f, braking > 10f ? braking / 100f : braking);

        Acceleration = CalculateRate(AccelerationCoefficient);
        Braking = CalculateRate(BrakingCoefficient);
    }

    /// <summary>
    /// a = m^(d*x^0.9) * k
    ///
    /// Mass is expressed in the same unit as VehicleParameters.Mass. The
    /// mass coefficient x is deliberately configurable because the formula
    /// is a gameplay tuning model rather than a dimensional SI equation.
    /// </summary>
    private float CalculateRate(float d)
    {
        if (d <= 0f)
            return 0f;

        double exponent = d * Math.Pow(MassCoefficient, 0.9d);
        double rate = Math.Pow(Mass, exponent) * TechnicalCondition;

        if (double.IsNaN(rate) || double.IsInfinity(rate))
            return 0f;

        return (float)Math.Clamp(rate, 0d, 1000d);
    }
}
