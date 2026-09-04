using System;

namespace RailDispatchMono.Core.Game.Train;

public abstract class Vehicle
{
    public Guid Id { get; }

    /// <summary>
    /// Stable position of this vehicle inside its current physical consist.
    /// This value is independent from the train travel direction (F7).
    /// </summary>
    public int CompositionOrder { get; internal set; } = -1;

    public VehicleParameters Parameters { get; }

    /// <summary>
    /// Static coupling-interface data for the rolling-stock definition.
    /// </summary>
    public CouplingSpecification Coupling { get; }

    /// <summary>
    /// Runtime connections at the vehicle's physical ends.
    /// </summary>
    public VehicleCouplingState CouplingState { get; }

    /// <summary>
    /// Intrinsic vehicle orientation. Front/Rear coupling ends are independent of travel direction.
    /// </summary>
    public VehicleOrientation Orientation { get; set; }

    protected Vehicle(
        VehicleParameters parameters,
        CouplingSpecification? coupling = null)
    {
        Id = Guid.NewGuid();
        Parameters = parameters;
        Coupling = coupling ?? CouplingSpecification.Default;
        CouplingState = new VehicleCouplingState();
        Orientation = VehicleOrientation.Forward;
    }
}
