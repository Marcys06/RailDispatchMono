using System;

namespace RailDispatchMono.Core.Game.Train;

public abstract class Vehicle
{
    public Guid Id { get; }

    public VehicleParameters Parameters { get; }

    protected Vehicle(VehicleParameters parameters)
    {
        Id = Guid.NewGuid();
        Parameters = parameters;
    }
}
