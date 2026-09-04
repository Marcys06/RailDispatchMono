using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>Rigid runtime coupling/decoupling. Dynamic coupler physics is deferred to a later version.</summary>
public sealed class CouplingService
{
    public const float CouplingDistance = CouplingGeometry.DefaultCouplingDistance;
    public const float AlignmentDot = CouplingGeometry.DefaultAlignmentDot;
    public const float DecouplingMaxSpeedKmh = 6f;

    public CouplingCheckResult CanCouple(
        Train firstTrain, int firstVehicleIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondVehicleIndex, VehicleEnd secondEnd)
    {
        if (firstTrain == null || secondTrain == null)
            return CouplingCheckResult.Fail(CouplingFailureReason.SameTrain);
        if (ReferenceEquals(firstTrain, secondTrain))
            return CouplingCheckResult.Fail(CouplingFailureReason.SameTrain);

        if (firstVehicleIndex < 0 || firstVehicleIndex >= firstTrain.Composition.Vehicles.Count ||
            secondVehicleIndex < 0 || secondVehicleIndex >= secondTrain.Composition.Vehicles.Count)
            return CouplingCheckResult.Fail(CouplingFailureReason.NotTrainBoundary);

        var first = firstTrain.Composition.Vehicles[firstVehicleIndex];
        var second = secondTrain.Composition.Vehicles[secondVehicleIndex];
        if (ReferenceEquals(first, second))
            return CouplingCheckResult.Fail(CouplingFailureReason.SameVehicle);
        if (first.CouplingState.IsOccupied(firstEnd) || second.CouplingState.IsOccupied(secondEnd))
            return CouplingCheckResult.Fail(CouplingFailureReason.EndOccupied);
        if (!AreCompatible(first.Coupling.Get(firstEnd), second.Coupling.Get(secondEnd)))
            return CouplingCheckResult.Fail(CouplingFailureReason.UnsupportedCoupler);
        if (!IsBoundary(firstTrain, firstVehicleIndex, firstEnd) || !IsBoundary(secondTrain, secondVehicleIndex, secondEnd))
            return CouplingCheckResult.Fail(CouplingFailureReason.NotTrainBoundary);

        Vector2 firstPoint = CouplingGeometry.GetEndpoint(firstTrain, firstVehicleIndex, firstEnd);
        Vector2 secondPoint = CouplingGeometry.GetEndpoint(secondTrain, secondVehicleIndex, secondEnd);
        if (Vector2.Distance(firstPoint, secondPoint) > CouplingDistance)
            return CouplingCheckResult.Fail(CouplingFailureReason.TooFarApart);
        if (!CouplingGeometry.AreFacing(firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd, AlignmentDot))
            return CouplingCheckResult.Fail(CouplingFailureReason.Misaligned);

        // Composition.Vehicles order is authoritative and must not be reversed.
        // Therefore a merge boundary is always Rear -> Front. Front -> Front
        // and Rear -> Rear would require reversing one consist's vehicle order.
        if (firstEnd != VehicleEnd.Rear || secondEnd != VehicleEnd.Front)
            return CouplingCheckResult.Fail(CouplingFailureReason.NotTrainBoundary);

        return CouplingCheckResult.Success;
    }

    public CouplingOperationResult Couple(
        TrainManager manager,
        Train firstTrain, int firstVehicleIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondVehicleIndex, VehicleEnd secondEnd)
    {
        var check = CanCouple(firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd);
        if (!check.Allowed)
            return CouplingOperationResult.Fail(check.Reason);

        // The validated connection is Rear -> Front, so the first train is the
        // ordered prefix and the second train is the ordered suffix. Do not use
        // locomotive presence to override the physical boundary order.
        Train leadingTrain = firstTrain;
        Train trailingTrain = secondTrain;

        // Coupling is a topology change, not a movement operation. Capture the
        // exact world positions before changing either composition and restore
        // those positions after the merge. No vehicle may be repositioned by
        // composition length, direction, locomotive type, or list index.
        var leadingVehicles = new List<Vehicle>(leadingTrain.Composition.Vehicles);
        var trailingVehicles = new List<Vehicle>(trailingTrain.Composition.Vehicles);
        var leadingPositions = leadingTrain.GetVehiclePositions();
        var trailingPositions = trailingTrain.GetVehiclePositions();
        var mergedPositions = new List<Vector2>(leadingPositions.Count + trailingPositions.Count);
        mergedPositions.AddRange(leadingPositions);
        mergedPositions.AddRange(trailingPositions);

        firstTrain.Speed = 0f;
        secondTrain.Speed = 0f;
        firstTrain.RadioStop();
        secondTrain.RadioStop();

        // Clear all old runtime links before rebuilding the merged chain. This
        // prevents stale links from blocking AddVehicle after a merge.
        foreach (var vehicle in leadingVehicles)
        {
            vehicle.CouplingState.Set(VehicleEnd.Front, null);
            vehicle.CouplingState.Set(VehicleEnd.Rear, null);
        }
        foreach (var vehicle in trailingVehicles)
        {
            vehicle.CouplingState.Set(VehicleEnd.Front, null);
            vehicle.CouplingState.Set(VehicleEnd.Rear, null);
        }

        leadingTrain.Composition.Clear();
        foreach (var vehicle in leadingVehicles)
            leadingTrain.Composition.AddVehicle(vehicle);
        foreach (var vehicle in trailingVehicles)
            leadingTrain.Composition.AddVehicle(vehicle);

        // Rebuild only the logical runtime chain. Never rebuild geometric offsets
        // from the new composition because that would teleport the suffix onto
        // the leading train's coordinate system.
        leadingTrain.Composition.RebuildRuntimeCouplings();
        leadingTrain.PreserveVehiclePositions(mergedPositions);
        leadingTrain.Speed = 0f;
        manager.ResetSignalStateAfterChange(leadingTrain);

        manager.Remove(trailingTrain);
        DebugManager.Log($"[COUPLING] Trains {firstTrain.Id.ToString()[..8]} and {secondTrain.Id.ToString()[..8]} coupled at rest without repositioning vehicles.");
        return CouplingOperationResult.Ok;
    }

    public CouplingOperationResult Decouple(TrainManager manager, Train train, Vehicle firstVehicle, VehicleEnd firstEnd)
    {
        if (manager == null || train == null || firstVehicle == null)
            return CouplingOperationResult.Fail(CouplingFailureReason.NotCoupled);

        float speedKmh = train.Speed * 3.6f;
        if (speedKmh >= DecouplingMaxSpeedKmh)
        {
            DebugManager.Log($"[COUPLING] Decouple rejected: train {train.Id.ToString()[..8]} speed {speedKmh:F1} km/h is not below {DecouplingMaxSpeedKmh:F0} km/h.");
            return CouplingOperationResult.Fail(CouplingFailureReason.NotCoupled);
        }

        var connection = firstVehicle.CouplingState.Get(firstEnd);
        if (connection == null)
            return CouplingOperationResult.Fail(CouplingFailureReason.NotCoupled);

        Vehicle secondVehicle = ReferenceEquals(connection.VehicleA, firstVehicle) ? connection.VehicleB : connection.VehicleA;
        int firstIndex = IndexOf(train, firstVehicle);
        int secondIndex = IndexOf(train, secondVehicle);
        if (firstIndex < 0 || secondIndex < 0)
            return CouplingOperationResult.Fail(CouplingFailureReason.NotCoupled);

        // The runtime connection is authoritative. The split point depends on
        // ordered vehicle indices, not on which physical end was selected.
        // This also handles a locomotive/wagon connection consistently.
        if (Math.Abs(firstIndex - secondIndex) != 1)
            return CouplingOperationResult.Fail(CouplingFailureReason.NotTrainBoundary);

        int splitIndex = Math.Max(firstIndex, secondIndex);
        var allPositions = train.GetVehiclePositions();
        var newHeadTransform = train.GetVehicleTransform(splitIndex);
        TrackConnections newDirection = DirectionFromAngle(newHeadTransform.Rotation);
        train.Speed = 0f;
        train.RadioStop();
        var splitComposition = train.Composition.Split(splitIndex);

        connection.VehicleA.CouplingState.Set(connection.EndA, null);
        connection.VehicleB.CouplingState.Set(connection.EndB, null);

        var remainingPositions = allPositions.GetRange(0, splitIndex);
        var detachedPositions = allPositions.GetRange(splitIndex, allPositions.Count - splitIndex);

        train.PreserveVehiclePositions(remainingPositions);

        var detached = new Train(newHeadTransform.Position, newDirection, 0f, splitComposition.Vehicles);
        detached.SetMap(manager.Map);
        detached.SetSignalController(train.GetSignalController() ?? new SignalController(manager.Map));
        detached.RadioStop();
        detached.PreserveVehiclePositions(detachedPositions);
        manager.RegisterCouplingTrain(detached);
        manager.ResetSignalStateAfterChange(train);
        manager.ResetSignalStateAfterChange(detached);

        DebugManager.Log($"[COUPLING] Train {train.Id.ToString()[..8]} decoupled; new train {detached.Id.ToString()[..8]} stopped without repositioning vehicles.");
        return CouplingOperationResult.Ok;
    }

    private static bool IsBoundary(Train train, int index, VehicleEnd end) =>
        (index == 0 && end == VehicleEnd.Front) ||
        (index == train.Composition.Vehicles.Count - 1 && end == VehicleEnd.Rear);

    private static bool AreCompatible(CouplerType first, CouplerType second) =>
        first != CouplerType.None && second != CouplerType.None && first == second;

    private static int IndexOf(Train train, Vehicle vehicle)
    {
        for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            if (ReferenceEquals(train.Composition.Vehicles[i], vehicle)) return i;
        return -1;
    }

    private static TrackConnections DirectionFromAngle(float angle)
    {
        while (angle > MathF.PI) angle -= MathF.Tau;
        while (angle < -MathF.PI) angle += MathF.Tau;
        float abs = MathF.Abs(angle);
        if (abs < MathF.PI / 4f || abs > 3f * MathF.PI / 4f)
            return angle >= 0f ? TrackConnections.East : TrackConnections.West;
        return angle >= 0f ? TrackConnections.South : TrackConnections.North;
    }
}
