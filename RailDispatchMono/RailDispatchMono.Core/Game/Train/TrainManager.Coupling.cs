using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Debug;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class TrainManager
{
    private KeyboardState _previousCouplingKeyboard;
    private Vehicle? _lastCoupledVehicle;
    private VehicleEnd _lastCoupledEnd;

    public CouplingService CouplingService { get; } = new();

    public CouplingCheckResult CanCouple(
        Train firstTrain, int firstVehicleIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondVehicleIndex, VehicleEnd secondEnd) =>
        CouplingService.CanCouple(firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd);

    public CouplingOperationResult Couple(
        Train firstTrain, int firstVehicleIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondVehicleIndex, VehicleEnd secondEnd) =>
        CouplingService.Couple(this, firstTrain, firstVehicleIndex, firstEnd, secondTrain, secondVehicleIndex, secondEnd);

    public CouplingOperationResult Decouple(Train train, Vehicle vehicle, VehicleEnd end) =>
        CouplingService.Decouple(this, train, vehicle, end);

    public const float CouplingSpeed3Kmh = 3f;
    public const float CouplingSpeed4Kmh = 4f;
    public const float CouplingSpeed5Kmh = 5f;
    public const SignalAspect CouplingSignalAspect = SignalAspect.Reserve3;

    public float CouplingCommandSpeedKmh { get; private set; } = CouplingSpeed5Kmh;

    public void HandleCouplingHotkeys()
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (IsNewCouplingKey(keyboard, Keys.F6)) CouplingCommandSpeedKmh = CouplingSpeed3Kmh;
        if (IsNewCouplingKey(keyboard, Keys.F7)) CouplingCommandSpeedKmh = CouplingSpeed4Kmh;
        if (IsNewCouplingKey(keyboard, Keys.F8)) CouplingCommandSpeedKmh = CouplingSpeed5Kmh;
        if (IsNewCouplingKey(keyboard, Keys.C)) ExecuteCouplingCommand();
        if (IsNewCouplingKey(keyboard, Keys.X)) ExecuteDecouplingCommand();

        _previousCouplingKeyboard = keyboard;
    }

    private bool IsNewCouplingKey(KeyboardState keyboard, Keys key) =>
        keyboard.IsKeyDown(key) && _previousCouplingKeyboard.IsKeyUp(key);

    private void ExecuteCouplingCommand()
    {
        CouplingCandidate? selected = null;
        float limitMps = CouplingCommandSpeedKmh / 3.6f;

        foreach (var train in _trains)
        {
            foreach (var candidate in GetCouplingCandidates(train))
            {
                if (!candidate.Check.Allowed ||
                    candidate.FirstTrain.Speed > limitMps ||
                    candidate.SecondTrain.Speed > limitMps)
                    continue;

                if (selected == null || candidate.Distance < selected.Value.Distance)
                    selected = candidate;
            }
        }

        if (!selected.HasValue)
        {
            DebugManager.Log($"[COUPLING] Command C rejected: no valid candidate at <= {CouplingCommandSpeedKmh:F0} km/h (S14 Rezerwowy 3).");
            return;
        }

        var candidateValue = selected.Value;
        var result = Couple(
            candidateValue.FirstTrain,
            candidateValue.FirstVehicleIndex,
            candidateValue.FirstEnd,
            candidateValue.SecondTrain,
            candidateValue.SecondVehicleIndex,
            candidateValue.SecondEnd);

        if (!result.Success)
        {
            DebugManager.Log($"[COUPLING] Command C failed: {result.Reason}.");
            return;
        }

        Vehicle firstVehicle = candidateValue.FirstTrain.Composition.Vehicles[candidateValue.FirstVehicleIndex];
        var connection = firstVehicle.CouplingState.Get(candidateValue.FirstEnd);
        if (connection != null)
        {
            if (ContainsVehicle(connection.VehicleA))
            {
                _lastCoupledVehicle = connection.VehicleA;
                _lastCoupledEnd = connection.EndA;
            }
            else
            {
                _lastCoupledVehicle = connection.VehicleB;
                _lastCoupledEnd = connection.EndB;
            }
        }

        DebugManager.Log($"[COUPLING] Command C executed at {CouplingCommandSpeedKmh:F0} km/h shunting limit / S14 Rezerwowy 3.");
    }

    private void ExecuteDecouplingCommand()
    {
        if (_lastCoupledVehicle != null)
        {
            Train? owner = FindTrainContaining(_lastCoupledVehicle);
            if (owner != null)
            {
                var result = Decouple(owner, _lastCoupledVehicle, _lastCoupledEnd);
                if (result.Success)
                {
                    _lastCoupledVehicle = null;
                    DebugManager.Log("[COUPLING] Command X decoupled the last coupling.");
                    return;
                }
            }
        }

        foreach (var train in _trains)
        {
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                var vehicle = train.Composition.Vehicles[i];
                foreach (var connection in vehicle.CouplingState.Connections())
                {
                    VehicleEnd end = ReferenceEquals(connection.VehicleA, vehicle) ? connection.EndA : connection.EndB;
                    var result = Decouple(train, vehicle, end);
                    if (result.Success)
                    {
                        DebugManager.Log("[COUPLING] Command X decoupled the first available runtime coupling.");
                        return;
                    }
                }
            }
        }

        DebugManager.Log("[COUPLING] Command X rejected: no runtime coupling found.");
    }

    private bool ContainsVehicle(Vehicle vehicle)
    {
        foreach (var train in _trains)
            if (train.Composition.Vehicles.Contains(vehicle)) return true;
        return false;
    }

    private Train? FindTrainContaining(Vehicle vehicle)
    {
        foreach (var train in _trains)
            if (train.Composition.Vehicles.Contains(vehicle)) return train;
        return null;
    }

    /// <summary>
    /// Returns all boundary-end pairs near the supplied train. The coupling service
    /// remains authoritative: candidates may be rejected for distance, alignment,
    /// coupler compatibility or occupied ends.
    /// </summary>
    public IReadOnlyList<CouplingCandidate> GetCouplingCandidates(Train train)
    {
        if (train == null) throw new ArgumentNullException(nameof(train));
        var result = new List<CouplingCandidate>();
        if (train.Composition.Vehicles.Count == 0)
            return result;

        for (int otherIndex = 0; otherIndex < _trains.Count; otherIndex++)
        {
            var otherTrain = _trains[otherIndex];
            if (ReferenceEquals(otherTrain, train) || otherTrain.Composition.Vehicles.Count == 0)
                continue;

            AddCandidateForEnd(result, train, 0, VehicleEnd.Front, otherTrain, 0, VehicleEnd.Front);
            AddCandidateForEnd(result, train, 0, VehicleEnd.Front, otherTrain,
                otherTrain.Composition.Vehicles.Count - 1, VehicleEnd.Rear);

            int lastIndex = train.Composition.Vehicles.Count - 1;
            AddCandidateForEnd(result, train, lastIndex, VehicleEnd.Rear, otherTrain, 0, VehicleEnd.Front);
            AddCandidateForEnd(result, train, lastIndex, VehicleEnd.Rear, otherTrain,
                otherTrain.Composition.Vehicles.Count - 1, VehicleEnd.Rear);
        }

        result.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
        return result;
    }

    private void AddCandidateForEnd(
        List<CouplingCandidate> result,
        Train firstTrain, int firstIndex, VehicleEnd firstEnd,
        Train secondTrain, int secondIndex, VehicleEnd secondEnd)
    {
        var firstPoint = CouplingGeometry.GetEndpoint(firstTrain, firstIndex, firstEnd);
        var secondPoint = CouplingGeometry.GetEndpoint(secondTrain, secondIndex, secondEnd);
        float distance = Vector2.Distance(firstPoint, secondPoint);
        var check = CanCouple(firstTrain, firstIndex, firstEnd, secondTrain, secondIndex, secondEnd);

        result.Add(new CouplingCandidate(
            firstTrain,
            firstIndex,
            firstEnd,
            secondTrain,
            secondIndex,
            secondEnd,
            firstPoint,
            secondPoint,
            distance,
            check));
    }

    /// <summary>Registers a train created by decoupling without applying spawn-block checks to its old position.</summary>
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
