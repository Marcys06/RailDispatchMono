using System;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>
/// Runtime bridge between TrainManager's signal sequencing and Train's
/// existing acceleration/braking implementation.
/// </summary>
public sealed partial class Train
{
    internal void SetEffectiveSignalSpeed(float speed)
    {
        speed = MathF.Max(0f, speed);
        _maxSpeed = speed;
        _lastSignalSpeed = speed;
        _targetSpeed = speed;
    }
}
