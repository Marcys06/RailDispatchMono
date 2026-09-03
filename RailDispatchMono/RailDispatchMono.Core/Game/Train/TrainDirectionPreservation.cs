using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Railway;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private readonly List<Vector2> _preservedVehiclePositions = new();

    /// <summary>
    /// Changes the train travel direction without rebuilding the consist geometry.
    /// While the train remains stopped, every vehicle keeps exactly the world
    /// coordinate it had immediately before the reversal.
    /// </summary>
    public void SetDirectionPreservingVehiclePositions(TrackConnections direction)
    {
        ValidateDirection(direction);

        _preservedVehiclePositions.Clear();
        for (int i = 0; i < Composition.Vehicles.Count; i++)
            _preservedVehiclePositions.Add(GetVehicleTransform(i).Position);

        Direction = direction;
        ResetCurveState();
        ResetTrajectory();
        ResetSignalState();
    }

    internal bool TryGetPreservedVehiclePosition(int vehicleIndex, out Vector2 position)
    {
        if (Speed <= MovementEpsilon &&
            vehicleIndex >= 0 &&
            vehicleIndex < _preservedVehiclePositions.Count &&
            _preservedVehiclePositions.Count == Composition.Vehicles.Count)
        {
            position = _preservedVehiclePositions[vehicleIndex];
            return true;
        }

        position = default;
        return false;
    }
}
