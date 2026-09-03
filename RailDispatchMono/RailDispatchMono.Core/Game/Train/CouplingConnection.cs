using System;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Runtime connection between two physical vehicle ends.</summary>
public sealed class CouplingConnection
{
    public Vehicle VehicleA { get; }
    public VehicleEnd EndA { get; }
    public Vehicle VehicleB { get; }
    public VehicleEnd EndB { get; }

    public CouplingConnection(Vehicle vehicleA, VehicleEnd endA, Vehicle vehicleB, VehicleEnd endB)
    {
        VehicleA = vehicleA ?? throw new ArgumentNullException(nameof(vehicleA));
        VehicleB = vehicleB ?? throw new ArgumentNullException(nameof(vehicleB));
        if (ReferenceEquals(vehicleA, vehicleB))
            throw new ArgumentException("A vehicle cannot be coupled to itself.");

        EndA = endA;
        EndB = endB;
    }

    public bool Contains(Vehicle vehicle) => ReferenceEquals(VehicleA, vehicle) || ReferenceEquals(VehicleB, vehicle);

    public bool Matches(Vehicle vehicleA, VehicleEnd endA, Vehicle vehicleB, VehicleEnd endB) =>
        (ReferenceEquals(VehicleA, vehicleA) && EndA == endA && ReferenceEquals(VehicleB, vehicleB) && EndB == endB) ||
        (ReferenceEquals(VehicleA, vehicleB) && EndA == endB && ReferenceEquals(VehicleB, vehicleA) && EndB == endA);
}
