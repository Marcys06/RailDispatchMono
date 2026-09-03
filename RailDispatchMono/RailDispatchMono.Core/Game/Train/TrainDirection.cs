using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private Vector2[]? _preservedVehicleOffsets;
    private float[]? _preservedVehicleRotations;

    internal void SetDirectionPreservingVehiclePositions(TrackConnections direction)
    {
        ValidateDirection(direction);

        int count = Composition.Vehicles.Count;
        _preservedVehicleOffsets = new Vector2[count];
        _preservedVehicleRotations = new float[count];

        Vector2 headPosition = Position;
        for (int i = 0; i < count; i++)
        {
            var transform = GetVehicleTransform(i);
            _preservedVehicleOffsets[i] = transform.Position - headPosition;
            _preservedVehicleRotations[i] = transform.Rotation;
        }

        Direction = direction;
        _lastSignal = null;
        _lastSignalSpeed = _maxSpeed;
        ResetCurveState();
        ResetTrajectory();
    }

    internal bool TryGetPreservedVehiclePosition(int vehicleIndex, out Vector2 position)
    {
        if (_preservedVehicleOffsets != null &&
            vehicleIndex >= 0 &&
            vehicleIndex < _preservedVehicleOffsets.Length)
        {
            position = Position + _preservedVehicleOffsets[vehicleIndex];
            return true;
        }

        position = default;
        return false;
    }

    internal bool TryGetPreservedVehicleRotation(int vehicleIndex, out float rotation)
    {
        if (_preservedVehicleRotations != null &&
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
        _preservedVehicleOffsets = null;
        _preservedVehicleRotations = null;
    }

    internal SignalController? GetSignalController() => _signalController;
}
