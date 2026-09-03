namespace RailDispatchMono.Core.Game.Train;

public sealed class Locomotive : Vehicle
{
    public LocomotiveType Type { get; }
    public string ShortName { get; }

    public Locomotive(
        LocomotiveType type,
        VehicleParameters parameters,
        string shortName)
        : base(parameters)
    {
        Type = type;
        ShortName = shortName;
    }
}