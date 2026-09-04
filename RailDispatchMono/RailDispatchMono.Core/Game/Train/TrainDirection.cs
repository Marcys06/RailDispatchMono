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

        Vector2[] vehiclePositions = CaptureCurrentVehiclePositions();

        Direction = direction;
        _isReversed = !_isReversed;

        _lastSignal = null;
        _signalSpeedLimit = Composition.EffectiveMaxSpeed;
        ResetCurveState();
        SeedTrajectoryFromVehiclePositions(vehiclePositions);
    }

    internal void RestoreTravelDirection(bool reversed)
    {
        Vector2[] vehiclePositions = CaptureCurrentVehiclePositions();

        _isReversed = reversed;
        _lastSignal = null;
        _signalSpeedLimit = Composition.EffectiveMaxSpeed;
        ResetCurveState();
        SeedTrajectoryFromVehiclePositions(vehiclePositions);
    }

    internal void PreserveVehiclePositions(IReadOnlyList<Vector2> vehiclePositions)
    {
        if (vehiclePositions.Count != Composition.Vehicles.Count)
            throw new ArgumentException("Vehicle position count must match the composition.", nameof(vehiclePositions));

        if (vehiclePositions.Count == 0)
        {
            ResetTrajectory();
            return;
        }

        Position = vehiclePositions[0];
        DistanceAlongTrack = 0f;
        TotalDistance = 0f;

        _vehicleOffsets = new Vector2[vehiclePositions.Count];
        for (int i = 0; i < vehiclePositions.Count; i++)
            _vehicleOffsets[i] = vehiclePositions[i] - Position;

        _lastSignal = null;
        _signalSpeedLimit = Composition.EffectiveMaxSpeed;
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

        for (int i = vehiclePositions.Count - 1; i >= 0; i--)
        {
            float distance = -GetMovementDistanceToVehicle(i);
            _trajectory.Add(new TrajectoryPoint(vehiclePositions[i], distance));
        }

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
