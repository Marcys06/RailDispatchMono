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

        if (!TryGetBoundaryOrder(firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd,
                out Train leadingTrain, out Train trailingTrain))
            return CouplingOperationResult.Fail(CouplingFailureReason.NotTrainBoundary);

        firstTrain.Speed = 0f;
        secondTrain.Speed = 0f;
        firstTrain.RadioStop();
        secondTrain.RadioStop();

        // F7 keeps a temporary rendering snapshot. A composition mutation makes
        // that snapshot invalid, so discard it before rebuilding the consist.
        firstTrain.ClearPreservedVehiclePositions();
        secondTrain.ClearPreservedVehiclePositions();

        var connection = new CouplingConnection(
            firstTrain.Composition.Vehicles[firstVehicleIndex], firstEnd,
            secondTrain.Composition.Vehicles[secondVehicleIndex], secondEnd);

        firstTrain.Composition.Vehicles[firstVehicleIndex].CouplingState.Set(firstEnd, connection);
        secondTrain.Composition.Vehicles[secondVehicleIndex].CouplingState.Set(secondEnd, connection);

        if (ReferenceEquals(leadingTrain, firstTrain))
        {
            foreach (var vehicle in trailingTrain.Composition.Vehicles)
                leadingTrain.Composition.AddVehicle(vehicle);
        }
        else
        {
            var vehicles = new List<Vehicle>(leadingTrain.Composition.Vehicles);
            leadingTrain.Composition.Clear();
            foreach (var vehicle in trailingTrain.Composition.Vehicles)
                leadingTrain.Composition.AddVehicle(vehicle);
            foreach (var vehicle in vehicles)
                leadingTrain.Composition.AddVehicle(vehicle);
        }

        leadingTrain.Speed = 0f;
        manager.ResetSignalStateAfterChange(leadingTrain);

        manager.Remove(trailingTrain);
        DebugManager.Log($"[COUPLING] Trains {firstTrain.Id.ToString()[..8]} and {secondTrain.Id.ToString()[..8]} coupled at rest.");
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
        VehicleEnd secondEnd = ReferenceEquals(connection.VehicleA, firstVehicle) ? connection.EndB : connection.EndA;
        int firstIndex = IndexOf(train, firstVehicle);
        int secondIndex = IndexOf(train, secondVehicle);
        if (firstIndex < 0 || secondIndex < 0)
            return CouplingOperationResult.Fail(CouplingFailureReason.NotCoupled);

        int splitIndex;
        if (firstIndex + 1 == secondIndex && firstEnd == VehicleEnd.Rear && secondEnd == VehicleEnd.Front)
            splitIndex = secondIndex;
        else if (secondIndex + 1 == firstIndex && secondEnd == VehicleEnd.Rear && firstEnd == VehicleEnd.Front)
            splitIndex = firstIndex;
        else
            return CouplingOperationResult.Fail(CouplingFailureReason.NotTrainBoundary);

        var newHeadTransform = train.GetVehicleTransform(splitIndex);
        TrackConnections newDirection = DirectionFromAngle(newHeadTransform.Rotation);
        train.Speed = 0f;
        train.RadioStop();
        train.ClearPreservedVehiclePositions();
        var splitComposition = train.Composition.Split(splitIndex);

        connection.VehicleA.CouplingState.Set(connection.EndA, null);
        connection.VehicleB.CouplingState.Set(connection.EndB, null);

        var detached = new Train(newHeadTransform.Position, newDirection, 0f, splitComposition.Vehicles);
        detached.SetMap(manager.Map);
        detached.SetSignalController(train.GetSignalController() ?? new SignalController(manager.Map));
        detached.RadioStop();
        manager.RegisterCouplingTrain(detached);
        manager.ResetSignalStateAfterChange(train);
        manager.ResetSignalStateAfterChange(detached);

        DebugManager.Log($"[COUPLING] Train {train.Id.ToString()[..8]} decoupled; new train {detached.Id.ToString()[..8]} stopped and signal context rebuilt.");
        return CouplingOperationResult.Ok;
    }

    private static bool IsBoundary(Train train, int index, VehicleEnd end) =>
        (index == 0 && end == VehicleEnd.Front) ||
        (index == train.Composition.Vehicles.Count - 1 && end == VehicleEnd.Rear);

    private static bool TryGetBoundaryOrder(
        Train firstTrain, int firstIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondIndex, VehicleEnd secondEnd,
        out Train leading, out Train trailing)
    {
        bool firstHasLocomotive = firstTrain.Composition.Locomotive != null;
        bool secondHasLocomotive = secondTrain.Composition.Locomotive != null;
        if (firstHasLocomotive != secondHasLocomotive)
        {
            leading = firstHasLocomotive ? firstTrain : secondTrain;
            trailing = firstHasLocomotive ? secondTrain : firstTrain;
            return true;
        }

        if (firstIndex == firstTrain.Composition.Vehicles.Count - 1 && firstEnd == VehicleEnd.Rear &&
            secondIndex == 0 && secondEnd == VehicleEnd.Front)
        {
            leading = firstTrain;
            trailing = secondTrain;
            return true;
        }
        if (secondIndex == secondTrain.Composition.Vehicles.Count - 1 && secondEnd == VehicleEnd.Rear &&
            firstIndex == 0 && firstEnd == VehicleEnd.Front)
        {
            leading = secondTrain;
            trailing = firstTrain;
            return true;
        }
        leading = null!;
        trailing = null!;
        return false;
    }

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
