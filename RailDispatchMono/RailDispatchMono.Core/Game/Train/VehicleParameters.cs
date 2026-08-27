namespace RailDispatchMono.Core.Game.Train;

public sealed class VehicleParameters
{
    public float MaxSpeed { get; set; }
    public float Acceleration { get; set; }
    public float Braking { get; set; }
    public float Mass { get; set; }
    public float Length { get; set; }

    public VehicleParameters(
        float maxSpeed,
        float acceleration,
        float braking,
        float mass,
        float length)
    {
        MaxSpeed = maxSpeed;
        Acceleration = acceleration;
        Braking = braking;
        Mass = mass;
        Length = length;
    }
}
