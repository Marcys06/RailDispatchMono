using System;

namespace RailDispatchMono.Core.Game.Train;

/// <summary>
/// Runtime bridge between TrainManager's signal sequencing and Train's
/// acceleration/braking implementation.
/// </summary>
public sealed partial class Train
{
    internal void SetEffectiveSignalSpeed(float speed)
    {
        _signalSpeedLimit = MathF.Max(0f, speed);
        _targetSpeed = _signalSpeedLimit;
    }
}
