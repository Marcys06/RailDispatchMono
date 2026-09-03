using System;

namespace RailDispatchMono.Core.Game.Train;

public abstract class Vehicle
{
    public Guid Id { get; }

    public VehicleParameters Parameters { get; }

    /// <summary>
    /// Static coupling-interface data for the rolling-stock definition.
    /// Connection state is not stored on the vehicle yet; 0.1.5 will own that state.
    /// </summary>
    public CouplingSpecification Coupling { get; }

    public VehicleOrientation Orientation { get; set; }

    protected Vehicle(
        VehicleParameters parameters,
        CouplingSpecification? coupling = null)
    {
        Id = Guid.NewGuid();
        Parameters = parameters;
        Coupling = coupling ?? CouplingSpecification.Default;
        Orientation = VehicleOrientation.Forward;
    }
}
