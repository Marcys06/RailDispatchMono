using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.RollingStock;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Locomotive : Vehicle
{
    public LocomotiveType Type { get; }
    public string ShortName { get; }
    public TractionType Traction { get; }
    public IReadOnlySet<TractionSystem> SupportedTractionSystems { get; }

    /// <summary>Optional locomotive-owned operational timetable.</summary>
    public LocomotiveSchedule? Schedule { get; set; }

    public Locomotive(
        LocomotiveType type,
        VehicleParameters parameters,
        string shortName,
        TractionType traction = TractionType.Diesel,
        IReadOnlySet<TractionSystem>? supportedTractionSystems = null)
        : base(parameters)
    {
        Type = type;
        ShortName = shortName;
        Traction = traction;
        SupportedTractionSystems = supportedTractionSystems ?? new HashSet<TractionSystem>();
    }

    public bool CanOperateOn(TractionSystem system)
        => Traction == TractionType.Diesel ||
           (system != TractionSystem.None && SupportedTractionSystems.Contains(system));
}
