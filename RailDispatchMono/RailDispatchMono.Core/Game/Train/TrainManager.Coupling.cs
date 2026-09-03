using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Debug;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class TrainManager
{
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
