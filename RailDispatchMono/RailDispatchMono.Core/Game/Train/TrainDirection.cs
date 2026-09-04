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

        if (vehiclePositions.Length > 0)
            Position = vehiclePositions[GetTravelHeadVehicleIndex()];

        SeedTrajectoryFromVehiclePositions(vehiclePositions);
    }

    internal void RestoreTravelDirection(bool reversed)
    {
        Vector2[] vehiclePositions = CaptureCurrentVehiclePositions();

        _isReversed = reversed;
        _lastSignal = null;
        _signalSpeedLimit = Composition.EffectiveMaxSpeed;
        ResetCurveState();

        if (vehiclePositions.Length > 0)
            Position = vehiclePositions[GetTravelHeadVehicleIndex()];

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

        int travelHeadIndex = GetTravelHeadVehicleIndex();
        Position = vehiclePositions[travelHeadIndex];
        DistanceAlongTrack = 0f;
        TotalDistance = 0f;

        _lastSignal = null;
        _signalSpeedLimit = Composition.EffectiveMaxSpeed;
        ResetCurveState();

        RailDispatchMono.Core.DebugManager.Train($"[TRAJECTORY] PRESERVE train={Id.ToString("N")[..8]} " +
            $"direction={Direction} reversed={_isReversed} headIndex={travelHeadIndex} " +
            $"position={FmtDebug(Position)} vehicles={Composition.Vehicles.Count}");
        for (int i = 0; i < vehiclePositions.Count; i++)
        {
            RailDispatchMono.Core.DebugManager.Train($"[TRAJECTORY] PRESERVE vehicle[{i}] " +
                $"id={Composition.Vehicles[i].Id.ToString("N")[..8]} " +
                $"input={FmtDebug(vehiclePositions[i])} " +
                $"movementDistance={GetMovementDistanceToVehicle(i):F3}");
        }

        SeedTrajectoryFromVehiclePositions(vehiclePositions);
    }

    private int GetTravelHeadVehicleIndex()
    {
        if (Composition.Vehicles.Count == 0)
            return 0;

        return _isReversed ? Composition.Vehicles.Count - 1 : 0;
    }

    private void SeedInitialTrajectoryFromComposition()
    {
        if (Composition.Vehicles.Count == 0)
            return;

        var positions = new Vector2[Composition.Vehicles.Count];
        Vector2 direction = DirectionToVector(Direction);
        float distance = 0f;

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = Position - direction * distance;
            distance += Composition.Vehicles[i].Parameters.Length;
        }

        SeedTrajectoryFromVehiclePositions(positions);
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

        var points = new List<TrajectoryPoint>(vehiclePositions.Count);
        for (int i = 0; i < vehiclePositions.Count; i++)
        {
            float distance = -GetMovementDistanceToVehicle(i);
            points.Add(new TrajectoryPoint(vehiclePositions[i], distance));
        }

        points.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
        _trajectory.AddRange(points);

        RailDispatchMono.Core.DebugManager.Train($"[TRAJECTORY] SEED train={Id.ToString("N")[..8]} " +
            $"direction={Direction} reversed={_isReversed} headIndex={GetTravelHeadVehicleIndex()} " +
            $"points={_trajectory.Count}");
        for (int i = 0; i < _trajectory.Count; i++)
        {
            var point = _trajectory[i];
            RailDispatchMono.Core.DebugManager.Train($"[TRAJECTORY] SEED point[{i}] " +
                $"distance={point.Distance:F3} position={FmtDebug(point.Position)}");
        }

        float historyExtension = MathF.Max(Length * 25.0f, 60.0f);
        TrajectoryPoint oldestPoint = _trajectory[0];
        float oldestDistance = oldestPoint.Distance;
        Vector2 historyPosition = oldestPoint.Position - DirectionToVector(Direction) * historyExtension;
        _trajectory.Insert(0, new TrajectoryPoint(
            historyPosition,
            oldestDistance - historyExtension));
    }

    internal float GetMovementDistanceToVehicle(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new ArgumentOutOfRangeException(nameof(vehicleIndex));

        if (!_isReversed)
        {
            float distance = 0f;
            for (int i = 0; i < vehicleIndex; i++)
                distance += Composition.Vehicles[i].Parameters.Length;
            return distance;
        }

        float reversedDistance = 0f;
        for (int i = Composition.Vehicles.Count - 1; i > vehicleIndex; i--)
            reversedDistance += Composition.Vehicles[i].Parameters.Length;
        return reversedDistance;
    }

    internal SignalController? GetSignalController() => _signalController;

    private static string FmtDebug(Vector2 value) => $"({value.X:F4},{value.Y:F4})";
}
