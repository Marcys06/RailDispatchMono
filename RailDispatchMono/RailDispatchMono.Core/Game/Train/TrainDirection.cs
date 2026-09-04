using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private bool _isReversed;

    internal void SetDirectionPreservingVehiclePositions(TrackConnections direction)
    {
        ValidateDirection(direction);

        // F7 changes only the travel direction. Capture the physical positions
        // before changing Direction so the consist can immediately continue from
        // the same world positions without reordering or teleporting vehicles.
        Vector2[] vehiclePositions = CaptureCurrentVehiclePositions();

        Direction = direction;
        _isReversed = !_isReversed;

        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        SeedTrajectoryFromVehiclePositions(vehiclePositions);
    }

    internal void RestoreTravelDirection(bool reversed)
    {
        // Save/load may restore a reversed consist after its physical positions
        // have already been reconstructed. Seed the trajectory from those exact
        // positions instead of collapsing it back to the locomotive position.
        Vector2[] vehiclePositions = CaptureCurrentVehiclePositions();

        _isReversed = reversed;
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        SeedTrajectoryFromVehiclePositions(vehiclePositions);
    }

    private Vector2[] CaptureCurrentVehiclePositions()
    {
        if (Composition.Vehicles.Count == 0)
            return Array.Empty<Vector2>();

        var positions = new Vector2[Composition.Vehicles.Count];
        for (int i = 0; i < positions.Length; i++)
            positions[i] = GetVehicleTransform(i).Position;
        return positions;
    }

    private void SeedTrajectoryFromVehiclePositions(IReadOnlyList<Vector2> vehiclePositions)
    {
        _trajectory.Clear();
        _totalTravelDistance = 0f;

        if (vehiclePositions.Count == 0)
        {
            _trajectory.Add(new TrajectoryPoint(Position, 0f));
            return;
        }

        // The locomotive remains the zero-distance anchor. Existing vehicles are
        // seeded as negative trajectory history according to their immutable
        // composition offsets. This is the path already occupied by the consist,
        // so the normal trajectory-based transform logic can continue smoothly
        // after F7 on straight track and through curves.
        for (int i = vehiclePositions.Count - 1; i >= 0; i--)
        {
            float distance = -GetMovementDistanceToVehicle(i);
            _trajectory.Add(new TrajectoryPoint(vehiclePositions[i], distance));
        }
    }

    internal float GetMovementDistanceToVehicle(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));

        float distance = 0f;
        for (int i = 0; i < vehicleIndex; i++)
            distance += Composition.Vehicles[i].Parameters.Length;
        return distance;
    }

    internal SignalController? GetSignalController() => _signalController;
}
