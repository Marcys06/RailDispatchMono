namespace RailDispatchMono.Core.Game.Train;

/// <summary>Type of mechanical coupling interface available on a vehicle end.</summary>
public enum CouplerType
{
    None,
    Screw,
    Automatic
}

/// <summary>Static rolling-stock coupling data for both physical vehicle ends.</summary>
public sealed class CouplingSpecification
{
    public CouplerType Front { get; }
    public CouplerType Rear { get; }

    public CouplingSpecification(
        CouplerType front = CouplerType.Screw,
        CouplerType rear = CouplerType.Screw)
    {
        Front = front;
        Rear = rear;
    }

    public CouplerType Get(VehicleEnd end) => end == VehicleEnd.Front ? Front : Rear;

    public static CouplingSpecification Default { get; } = new();
}
