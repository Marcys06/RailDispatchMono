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

        // F7 changes the travel direction of the complete physical consist.
        // The logical composition order is unchanged, but the physical leading
        // vehicle becomes the vehicle that was at the opposite end of the consist.
        // Remapping preserved coordinates in reverse index order prevents vehicles
        // from crossing through one another when movement resumes.
        _isReversed = !_isReversed;
        Direction = direction;

        _preservedVehiclePositions = new Vector2[count];
        _preservedVehicleRotations = new float[count];
        for (int i = 0; i < count; i++)
        {
            int sourceIndex = count - 1 - i;
            _preservedVehiclePositions[i] = transforms[sourceIndex].Position;
            _preservedVehicleRotations[i] = NormalizeAngle(transforms[sourceIndex].Rotation + MathF.PI);
        }

        // The simulation reference point is always the physical front of the
        // consist. After F7 it therefore moves to the former last vehicle's
        // position. The consist reverses as one formation; no vehicle overtakes
        // another during the transition.
        Position = _preservedVehiclePositions[0];
        DistanceAlongTrack = 0f;
        TotalDistance = 0f;

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

    private static float NormalizeAngle(float angle)
    {
        while (angle > MathF.PI)
            angle -= MathF.Tau;
        while (angle <= -MathF.PI)
            angle += MathF.Tau;
        return angle;
    }
}
