namespace RailDispatchMono.Core.Game.Train;

/// <summary>Type of mechanical coupling interface available on a vehicle end.</summary>
public enum CouplerType
{
    None,
    Screw,
    Automatic
}

/// <summary>
/// Static rolling-stock coupling data. Runtime connection state is intentionally
/// not stored here; coupling/decoupling mechanics are planned for 0.1.5.
/// </summary>
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

    public static CouplingSpecification Default { get; } = new();
}
