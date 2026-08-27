namespace RailDispatchMono.Core.Game.Train;

public sealed class Locomotive : Vehicle
{
    public LocomotiveType Type { get; }

    public Locomotive(
        LocomotiveType type,
        VehicleParameters parameters)
        : base(parameters)
    {
        Type = type;
    }
}
