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

    /// <summary>
    /// Rebinds the runtime geometry to the exact world positions already occupied
    /// by the vehicles. Composition changes are state changes, not movement, so
    /// this method must never calculate a new position from vehicle index/length.
    /// </summary>
    internal void PreserveVehiclePositions(IReadOnlyList<Vector2> vehiclePositions)
    {
        if (vehiclePositions.Count != Composition.Vehicles.Count)
            throw new ArgumentException("Vehicle position count must match the composition.", nameof(vehiclePositions));

        if (vehiclePositions.Count == 0)
        {
            ResetTrajectory();
            return;
        }

        // The first vehicle remains the train head. Moving the train anchor to
        // its already occupied position makes all subsequent offsets relative to
        // the same physical state instead of reconstructing the consist from
        // length/direction and teleporting vehicles.
        Position = vehiclePositions[0];
        DistanceAlongTrack = 0f;
        TotalDistance = 0f;

        _vehicleOffsets = new Vector2[vehiclePositions.Count];
        for (int i = 0; i < vehiclePositions.Count; i++)
            _vehicleOffsets[i] = vehiclePositions[i] - Position;

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

        // When the consist starts moving after F7, the target distance for the
        // last vehicle immediately moves past the oldest seeded point. Without
        // an extrapolated history point, GetVehicleTransform() falls back to a
        // rigid offset and that can place the last wagon on the wrong side of the
        // consist. Extend the already occupied path in the direction opposite to
        // travel so every vehicle has continuous history from the first frame.
        float historyExtension = MathF.Max(Length * 25.0f, 60.0f);
        int lastVehicleIndex = vehiclePositions.Count - 1;
        float oldestDistance = -GetMovementDistanceToVehicle(lastVehicleIndex);
        Vector2 oldestPosition = vehiclePositions[lastVehicleIndex];
        Vector2 historyPosition = oldestPosition - DirectionToVector(Direction) * historyExtension;
        _trajectory.Insert(0, new TrajectoryPoint(
            historyPosition,
            oldestDistance - historyExtension));
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
