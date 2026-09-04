using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Railway;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class TrainManager
{
    private KeyboardState _previousCouplingKeyboard;

    public CouplingService CouplingService { get; } = new();

    public CouplingCheckResult CanCouple(Train firstTrain, int firstVehicleIndex, VehicleEnd firstEnd, Train secondTrain, int secondVehicleIndex, VehicleEnd secondEnd) =>
        CouplingService.CanCouple(firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd);

    public CouplingOperationResult Couple(Train firstTrain, int firstVehicleIndex, VehicleEnd firstEnd, Train secondTrain, int secondVehicleIndex, VehicleEnd secondEnd) =>
        CouplingService.Couple(this, firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd);

    public CouplingOperationResult Decouple(Train train, Vehicle vehicle, VehicleEnd end) => CouplingService.Decouple(this, train, vehicle, end);

    public const float CouplingSpeedKmh = 6f;
    public const float ManualShuntingSpeedKmh = 3f;
    public const SignalAspect CouplingSignalAspect = SignalAspect.Reserve3;
    public float CouplingCommandSpeedKmh => CouplingSpeedKmh;

    public void HandleCouplingHotkeys(Vector2? cursorWorldPosition = null)
    {
        KeyboardState keyboard = Keyboard.GetState();
        if (IsNewCouplingKey(keyboard, Keys.C)) ExecuteCouplingCommand();
        if (IsNewCouplingKey(keyboard, Keys.X)) ExecuteDecouplingCommand(cursorWorldPosition);
        if (IsNewCouplingKey(keyboard, Keys.F7)) ExecuteLocomotiveReverseCommand(cursorWorldPosition);
        _previousCouplingKeyboard = keyboard;
    }

    private bool IsNewCouplingKey(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && _previousCouplingKeyboard.IsKeyUp(key);

    private void ExecuteLocomotiveReverseCommand(Vector2? cursorWorldPosition)
    {
        if (!cursorWorldPosition.HasValue)
        {
            DebugManager.Log("[TRAIN] Command F7 rejected: cursor position unavailable.");
            return;
        }

        Train? train = GetTrainAtWorldPosition(cursorWorldPosition.Value);
        if (train == null)
        {
            DebugManager.Log("[TRAIN] Command F7 rejected: no train under the cursor.");
            return;
        }

        if (train.Speed != 0f)
        {
            DebugManager.Log($"[TRAIN] Command F7 rejected: locomotive reversal requires 0 km/h (current {train.Speed * 3.6f:F1} km/h).");
            return;
        }

        var locomotive = train.Composition.Locomotive;
        if (locomotive == null)
        {
            DebugManager.Log("[TRAIN] Command F7 rejected: selected train has no locomotive.");
            return;
        }

        train.SetDirectionPreservingVehiclePositions(GetOppositeDirection(train.Direction));
        ResetSignalStateAfterChange(train);

        DebugManager.Log($"[TRAIN] Command F7: travel direction reversed for train {train.Id.ToString()[..8]}; composition order and vehicle coordinates unchanged, direction={train.Direction}, reversed={train.IsReversed}.");
    }

    internal void ResetSignalStateAfterChange(Train train)
    {
        Signal? nextSignal = train.GetNextSignal();
        float currentLimit = nextSignal != null
            ? GetSignalSpeedLimit(train, nextSignal)
            : GetEffectiveMaxSpeed(train);

        _signalSpeedStates[train.Id] = new SignalSpeedState
        {
            ApproachingSignal = nextSignal,
            CurrentLimit = currentLimit
        };

        train.ResetSignalState();
        train.SetEffectiveSignalSpeed(currentLimit);
    }

    private static TrackConnections GetOppositeDirection(TrackConnections direction) => direction switch
    {
        TrackConnections.North => TrackConnections.South,
        TrackConnections.South => TrackConnections.North,
        TrackConnections.East => TrackConnections.West,
        TrackConnections.West => TrackConnections.East,
        _ => TrackConnections.None
    };

    private void ExecuteCouplingCommand()
    {
        CouplingCandidate? selected = null;
        float limitMps = CouplingSpeedKmh / 3.6f;
        foreach (var train in _trains)
        foreach (var candidate in GetCouplingCandidates(train))
        {
            if (!candidate.Check.Allowed || candidate.FirstTrain.Speed > limitMps || candidate.SecondTrain.Speed > limitMps) continue;
            if (selected == null || candidate.Distance < selected.Value.Distance) selected = candidate;
        }
        if (!selected.HasValue)
        {
            DebugManager.Log($"[COUPLING] Command C rejected: no valid candidate at <= {CouplingSpeedKmh:F0} km/h (fixed shunting limit).");
            return;
        }
        var candidateValue = selected.Value;
        var result = Couple(candidateValue.FirstTrain, candidateValue.FirstVehicleIndex, candidateValue.FirstEnd, candidateValue.SecondTrain, candidateValue.SecondVehicleIndex, candidateValue.SecondEnd);
        if (!result.Success)
        {
            DebugManager.Log($"[COUPLING] Command C failed: {result.Reason}.");
            return;
        }
        DebugManager.Log($"[COUPLING] Command C executed at fixed {CouplingSpeedKmh:F0} km/h shunting limit.");
    }

    private void ExecuteDecouplingCommand(Vector2? cursorWorldPosition)
    {
        DebugManager.Log("[COUPLING] Command X received: selecting runtime coupling at cursor.");

        if (!cursorWorldPosition.HasValue)
        {
            DebugManager.Log("[COUPLING] Command X rejected: cursor position unavailable.");
            return;
        }

        var target = FindDecouplingTarget(cursorWorldPosition.Value);
        if (!target.HasValue)
        {
            DebugManager.Log("[COUPLING] Command X rejected: no wagon with a runtime coupling under the cursor.");
            return;
        }

        var (train, vehicle, vehicleIndex, end) = target.Value;
        float speedKmh = train.Speed * 3.6f;
        DebugManager.Log($"[COUPLING] Command X target: train {train.Id.ToString()[..8]}, wagon index {vehicleIndex}, end {end}, speed {speedKmh:F1} km/h.");

        if (speedKmh >= CouplingSpeedKmh)
        {
            DebugManager.Log($"[COUPLING] Command X rejected: decoupling requires train speed < {CouplingSpeedKmh:F0} km/h (current {speedKmh:F1} km/h).");
            return;
        }

        var result = Decouple(train, vehicle, end);
        if (!result.Success)
        {
            DebugManager.Log($"[COUPLING] Command X failed: {result.Reason}.");
            return;
        }

        DebugManager.Log($"[COUPLING] Command X success: train {train.Id.ToString()[..8]} was split at wagon index {vehicleIndex} / {end}. New detached train was created and stopped.");
    }

    private (Train Train, Vehicle Vehicle, int VehicleIndex, VehicleEnd End)? FindDecouplingTarget(Vector2 cursorWorldPosition)
    {
        float bestDistance = float.MaxValue;
        (Train Train, Vehicle Vehicle, int VehicleIndex, VehicleEnd End)? best = null;

        foreach (var train in _trains)
        {
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                var vehicle = train.Composition.Vehicles[i];
                if (vehicle is not Wagon)
                    continue;

                var connections = vehicle.CouplingState.Connections().ToList();
                if (connections.Count == 0)
                    continue;

                var preferredConnection = connections.FirstOrDefault(connection =>
                    ReferenceEquals(connection.VehicleA, vehicle) ? connection.EndA == VehicleEnd.Rear : connection.EndB == VehicleEnd.Rear)
                    ?? connections[0];

                float detectionRadius = MathF.Max(0.6f, vehicle.Parameters.Length * 0.5f);
                float distance = Vector2.Distance(train.GetVehicleTransform(i).Position, cursorWorldPosition);
                if (distance >= detectionRadius || distance >= bestDistance)
                    continue;

                VehicleEnd end = ReferenceEquals(preferredConnection.VehicleA, vehicle)
                    ? preferredConnection.EndA
                    : preferredConnection.EndB;

                bestDistance = distance;
                best = (train, vehicle, i, end);
            }
        }

        return best;
    }

    public IReadOnlyList<CouplingCandidate> GetCouplingCandidates(Train train)
    {
        if (train == null) throw new ArgumentNullException(nameof(train));
        var result = new List<CouplingCandidate>();
        if (train.Composition.Vehicles.Count == 0) return result;

        int lastIndex = train.Composition.Vehicles.Count - 1;
        for (int otherIndex = 0; otherIndex < _trains.Count; otherIndex++)
        {
            var otherTrain = _trains[otherIndex];
            if (ReferenceEquals(otherTrain, train) || otherTrain.Composition.Vehicles.Count == 0) continue;

            int otherLastIndex = otherTrain.Composition.Vehicles.Count - 1;

            // Only order-preserving boundaries are candidates: Rear -> Front.
            // The reverse ordering is represented by iterating the other train.
            AddCandidateForEnd(result, train, lastIndex, VehicleEnd.Rear, otherTrain, 0, VehicleEnd.Front);
        }

        result.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
        return result;
    }

    private void AddCandidateForEnd(List<CouplingCandidate> result, Train firstTrain, int firstIndex, VehicleEnd firstEnd, Train secondTrain, int secondIndex, VehicleEnd secondEnd)
    {
        var firstPoint = CouplingGeometry.GetEndpoint(firstTrain, firstIndex, firstEnd);
        var secondPoint = CouplingGeometry.GetEndpoint(secondTrain, secondIndex, secondEnd);
        float distance = Vector2.Distance(firstPoint, secondPoint);
        var check = CanCouple(firstTrain, firstIndex, firstEnd, secondTrain, secondIndex, secondEnd);
        result.Add(new CouplingCandidate(firstTrain, firstIndex, firstEnd, secondTrain, secondIndex, secondEnd, firstPoint, secondPoint, distance, check));
    }

    internal void RegisterCouplingTrain(Train train)
    {
        if (train == null) throw new ArgumentNullException(nameof(train));
        if (_trains.Contains(train) || _trainsToAdd.Contains(train)) return;
        train.SetMap(_map);
        _trainsToAdd.Add(train);
        _signalSpeedStates[train.Id] = new SignalSpeedState { CurrentLimit = GetEffectiveMaxSpeed(train) };
        train.SetEffectiveSignalSpeed(GetEffectiveMaxSpeed(train));
        DebugManager.Log($"[COUPLING] Registered split train {train.Id.ToString()[..8]}.");
    }
}
