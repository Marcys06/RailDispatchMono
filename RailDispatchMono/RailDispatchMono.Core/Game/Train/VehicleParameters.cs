public class VehicleParameters
{
    public float MaxSpeed { get; }
    public float Acceleration { get; }
    public float Braking { get; }  
    public float Mass { get; }
    public float Length { get; }

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