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

        Train leadingTrain = firstTrain;
        Train trailingTrain = secondTrain;

        var leadingVehicles = new List<Vehicle>(leadingTrain.Composition.Vehicles);
        var trailingVehicles = new List<Vehicle>(trailingTrain.Composition.Vehicles);
        var leadingPositions = leadingTrain.GetVehiclePositions();
        var trailingPositions = trailingTrain.GetVehiclePositions();
        var mergedPositions = new List<Vector2>(leadingPositions.Count + trailingPositions.Count);
        mergedPositions.AddRange(leadingPositions);
        mergedPositions.AddRange(trailingPositions);

        DebugManager.Train($"[COUPLING] BEGIN first={ShortId(firstTrain)} second={ShortId(secondTrain)} " +
                           $"firstEnd={firstEnd} secondEnd={secondEnd} " +
                           $"firstIndex={firstVehicleIndex} secondIndex={secondVehicleIndex}");
        LogTrainState("FIRST-BEFORE", firstTrain, leadingVehicles, leadingPositions);
        LogTrainState("SECOND-BEFORE", secondTrain, trailingVehicles, trailingPositions);
        DebugManager.Train($"[COUPLING] MERGE INPUT positions={mergedPositions.Count} " +
                           $"leadingCount={leadingVehicles.Count} trailingCount={trailingVehicles.Count}");

        firstTrain.Speed = 0f;
        secondTrain.Speed = 0f;
        firstTrain.RadioStop();
        secondTrain.RadioStop();

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

        DebugManager.Train($"[COUPLING] MERGED compositionCount={leadingTrain.Composition.Vehicles.Count} " +
                           $"direction={leadingTrain.Direction} reversed={leadingTrain.IsReversed} " +
                           $"position={Fmt(leadingTrain.Position)}");
        LogComposition("MERGED-BEFORE-PRESERVE", leadingTrain);

        leadingTrain.Composition.RebuildRuntimeCouplings();
        DebugManager.Train("[COUPLING] RUNTIME CONNECTIONS REBUILT");
        LogComposition("MERGED-AFTER-LINKS", leadingTrain);

        leadingTrain.PreserveVehiclePositions(mergedPositions);
        leadingTrain.Speed = 0f;
        manager.ResetSignalStateAfterChange(leadingTrain);

        LogTrainState("MERGED-AFTER-PRESERVE", leadingTrain, leadingTrain.Composition.Vehicles, mergedPositions);
        LogTrajectory("MERGED-AFTER-PRESERVE", leadingTrain);

        manager.Remove(trailingTrain);
        DebugManager.Train($"[COUPLING] END survivor={ShortId(leadingTrain)} removed={ShortId(trailingTrain)} " +
                           $"position={Fmt(leadingTrain.Position)} direction={leadingTrain.Direction} " +
                           $"reversed={leadingTrain.IsReversed} speed={leadingTrain.Speed:F3}");
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

        if (Math.Abs(firstIndex - secondIndex) != 1)
            return CouplingOperationResult.Fail(CouplingFailureReason.NotTrainBoundary);

        int splitIndex = Math.Max(firstIndex, secondIndex);
        var allPositions = train.GetVehiclePositions();
        var newHeadTransform = train.GetVehicleTransform(splitIndex);
        float travelRotation = newHeadTransform.Rotation;
        if (train.Composition.Vehicles[splitIndex].Orientation == VehicleOrientation.Reverse)
            travelRotation -= MathF.PI;
        TrackConnections newDirection = DirectionFromAngle(travelRotation);
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

        DebugManager.Log($"[COUPLING] Decouple train {train.Id.ToString()[..8]} -> new train {detached.Id.ToString()[..8]} stopped without repositioning vehicles.");
        return CouplingOperationResult.Ok;
    }

    private static void LogTrainState(string phase, Train train, IReadOnlyList<Vehicle> vehicles, IReadOnlyList<Vector2> positions)
    {
        DebugManager.Train($"[COUPLING] {phase} train={ShortId(train)} " +
                           $"direction={train.Direction} reversed={train.IsReversed} " +
                           $"position={Fmt(train.Position)} speed={train.Speed:F3} " +
                           $"length={train.Length:F3} vehicles={vehicles.Count}");

        for (int i = 0; i < vehicles.Count; i++)
        {
            var vehicle = vehicles[i];
            Vector2 position = i < positions.Count ? positions[i] : Vector2.Zero;
            DebugManager.Train($"[COUPLING] {phase} vehicle[{i}] id={ShortId(vehicle.Id)} " +
                               $"type={vehicle.GetType().Name} order={vehicle.CompositionOrder} " +
                               $"orientation={vehicle.Orientation} length={vehicle.Parameters.Length:F3} " +
                               $"position={Fmt(position)} distance={train.GetDistanceToVehicle(i):F3}");
        }
    }

    private static void LogComposition(string phase, Train train)
    {
        for (int i = 0; i < train.Composition.Vehicles.Count; i++)
        {
            var vehicle = train.Composition.Vehicles[i];
            var transform = train.GetVehicleTransform(i);
            DebugManager.Train($"[COUPLING] {phase} vehicle[{i}] id={ShortId(vehicle.Id)} " +
                               $"order={vehicle.CompositionOrder} type={vehicle.GetType().Name} " +
                               $"orientation={vehicle.Orientation} position={Fmt(transform.Position)} " +
                               $"rotation={transform.Rotation:F3} distance={train.GetDistanceToVehicle(i):F3}");
        }
    }

    private static void LogTrajectory(string phase, Train train)
    {
        var history = train.GetTrajectoryHistory();
        DebugManager.Train($"[COUPLING] {phase} trajectoryPoints={history.Count} " +
                           $"lastDistance={(history.Count > 0 ? history[^1].Distance : 0f):F3}");
        for (int i = 0; i < history.Count; i++)
        {
            var point = history[i];
            DebugManager.Train($"[COUPLING] {phase} trajectory[{i}] distance={point.Distance:F3} position={Fmt(point.Position)}");
        }
    }

    private static string ShortId(Train train) => train.Id.ToString("N")[..8];
    private static string ShortId(Guid id) => id.ToString("N")[..8];
    private static string Fmt(Vector2 value) => $"({value.X:F4},{value.Y:F4})";

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
