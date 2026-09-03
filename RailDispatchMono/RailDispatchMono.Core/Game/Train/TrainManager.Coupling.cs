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
    private Vehicle? _lastCoupledVehicle;
    private VehicleEnd _lastCoupledEnd;

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
        _previousCouplingKeyboard = keyboard;
    }

    private bool IsNewCouplingKey(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && _previousCouplingKeyboard.IsKeyUp(key);

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
            DebugManager.Log($"[COUPLING] Command C rejected: no valid candidate at <= {CouplingSpeedKmh:F0} km/h (S14 Rezerwowy 3).");
            return;
        }
        var candidateValue = selected.Value;
        var result = Couple(candidateValue.FirstTrain, candidateValue.FirstVehicleIndex, candidateValue.FirstEnd, candidateValue.SecondTrain, candidateValue.SecondVehicleIndex, candidateValue.SecondEnd);
        if (!result.Success)
        {
            DebugManager.Log($"[COUPLING] Command C failed: {result.Reason}.");
            return;
        }
        Vehicle firstVehicle = candidateValue.FirstTrain.Composition.Vehicles[candidateValue.FirstVehicleIndex];
        var connection = firstVehicle.CouplingState.Get(candidateValue.FirstEnd);
        if (connection != null)
        {
            if (ContainsVehicle(connection.VehicleA)) { _lastCoupledVehicle = connection.VehicleA; _lastCoupledEnd = connection.EndA; }
            else { _lastCoupledVehicle = connection.VehicleB; _lastCoupledEnd = connection.EndB; }
        }
        DebugManager.Log($"[COUPLING] Command C executed at fixed {CouplingSpeedKmh:F0} km/h shunting limit / S14 Rezerwowy 3.");
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

        _lastCoupledVehicle = null;
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

                foreach (var connection in vehicle.CouplingState.Connections())
                {
                    VehicleEnd end = ReferenceEquals(connection.VehicleA, vehicle) ? connection.EndA : connection.EndB;
                    float distance = Vector2.Distance(train.GetVehicleTransform(i).Position, cursorWorldPosition);
                    if (distance >= bestDistance)
                        continue;

                    bestDistance = distance;
                    best = (train, vehicle, i, end);
                }
            }
        }

        return best;
    }

    private static int FindVehicleIndex(Train train, Vehicle vehicle)
    {
        for (int i = 0; i < train.Composition.Vehicles.Count; i++)
        {
            if (ReferenceEquals(train.Composition.Vehicles[i], vehicle)) return i;
        }

        return -1;
    }

    private bool ContainsVehicle(Vehicle vehicle) => _trains.Any(train => train.Composition.Vehicles.Any(v => ReferenceEquals(v, vehicle)));

    public IReadOnlyList<CouplingCandidate> GetCouplingCandidates(Train train)
    {
        if (train == null) throw new ArgumentNullException(nameof(train));
        var result = new List<CouplingCandidate>();
        if (train.Composition.Vehicles.Count == 0) return result;
        for (int otherIndex = 0; otherIndex < _trains.Count; otherIndex++)
        {
            var otherTrain = _trains[otherIndex];
            if (ReferenceEquals(otherTrain, train) || otherTrain.Composition.Vehicles.Count == 0) continue;
            AddCandidateForEnd(result, train, 0, VehicleEnd.Front, otherTrain, 0, VehicleEnd.Front);
            AddCandidateForEnd(result, train, 0, VehicleEnd.Front, otherTrain, otherTrain.Composition.Vehicles.Count - 1, VehicleEnd.Rear);
            int lastIndex = train.Composition.Vehicles.Count - 1;
            AddCandidateForEnd(result, train, lastIndex, VehicleEnd.Rear, otherTrain, 0, VehicleEnd.Front);
            AddCandidateForEnd(result, train, lastIndex, VehicleEnd.Rear, otherTrain, otherTrain.Composition.Vehicles.Count - 1, VehicleEnd.Rear);
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
        _signalSpeedStates[train.Id] = new SignalSpeedState { CurrentLimit = train.Composition.EffectiveMaxSpeed };
        train.SetEffectiveSignalSpeed(train.Composition.EffectiveMaxSpeed);
        DebugManager.Log($"[COUPLING] Registered split train {train.Id.ToString()[..8]}.");
    }
}
