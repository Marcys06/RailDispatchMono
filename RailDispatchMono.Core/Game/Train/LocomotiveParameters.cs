namespace RailDispatchMono.Core.Game.Train;

/// <summary>
/// Physical parameters specific to a locomotive, including installed traction power.
/// </summary>
public sealed class LocomotiveParameters : VehicleParameters
{
    public float PowerMW { get; }

    public LocomotiveParameters(
        float maxSpeedKmh,
        float accelerationMps2,
        float decelerationMps2,
        float massTons,
        float lengthMeters,
        float powerMW,
        float massCoefficient = 0.01f,
        float technicalCondition = 1.0f)
        : base(
            maxSpeedKmh / 3.6f,
            accelerationMps2,
            decelerationMps2,
            massTons * 1000f,
            lengthMeters / 10f,
            massCoefficient,
            technicalCondition)
    {
        PowerMW = Math.Max(0f, powerMW);
    }
}
