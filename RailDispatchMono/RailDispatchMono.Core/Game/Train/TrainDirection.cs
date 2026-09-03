using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private bool _isReversed;
    private Vector2[]? _preservedVehiclePositions;
    private float[]? _preservedVehicleRotations;

    internal void SetDirectionPreservingVehiclePositions(TrackConnections direction)
    {
        ValidateDirection(direction);

        int count = Composition.Vehicles.Count;
        if (count == 0)
        {
            Direction = direction;
            _isReversed = false;
            ClearPreservedVehiclePositions();
            _lastSignal = null;
            _lastSignalSpeed = _maxSpeed;
            ResetCurveState();
            ResetTrajectory();
            return;
        }

        var transforms = new (Vector2 Position, float Rotation)[count];
        for (int i = 0; i < count; i++)
            transforms[i] = GetVehicleTransform(i);

        // Position remains anchored to the same physical first vehicle. Direction
        // is the independent travel state. Reversing therefore never changes the
        // composition order and never moves the formation at the instant of F7.
        _isReversed = !_isReversed;
        Direction = direction;

        _preservedVehiclePositions = new Vector2[count];
        _preservedVehicleRotations = new float[count];
        for (int i = 0; i < count; i++)
        {
            _preservedVehiclePositions[i] = transforms[i].Position;
            _preservedVehicleRotations[i] = transforms[i].Rotation;
        }

        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        ResetTrajectory();
    }

    internal void RestoreTravelDirection(bool reversed)
    {
        _isReversed = reversed;
        ClearPreservedVehiclePositions();
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        ResetTrajectory();
    }

    internal float GetMovementDistanceToVehicle(int vehicleIndex)
    {
        if (vehicleIndex < 0 || vehicleIndex >= Composition.Vehicles.Count)
            throw new System.ArgumentOutOfRangeException(nameof(vehicleIndex));

        float distance = 0f;
        for (int i = 0; i < vehicleIndex; i++)
            distance += Composition.Vehicles[i].Parameters.Length;
        distance += Composition.Vehicles[vehicleIndex].Parameters.Length * 0.5f;
        return distance;
    }

    internal bool TryGetPreservedVehiclePosition(int vehicleIndex, out Vector2 position)
    {
        if (_totalTravelDistance <= 0.00001f &&
            _preservedVehiclePositions != null &&
            vehicleIndex >= 0 &&
            vehicleIndex < _preservedVehiclePositions.Length)
        {
            position = _preservedVehiclePositions[vehicleIndex];
            return true;
        }

        position = default;
        return false;
    }

    internal bool TryGetPreservedVehicleRotation(int vehicleIndex, out float rotation)
    {
        if (_totalTravelDistance <= 0.00001f &&
            _preservedVehicleRotations != null &&
            vehicleIndex >= 0 &&
            vehicleIndex < _preservedVehicleRotations.Length)
        {
            rotation = _preservedVehicleRotations[vehicleIndex];
            return true;
        }

        rotation = default;
        return false;
    }

    internal void ClearPreservedVehiclePositions()
    {
        _preservedVehiclePositions = null;
        _preservedVehicleRotations = null;
    }

    internal SignalController? GetSignalController() => _signalController;
}
