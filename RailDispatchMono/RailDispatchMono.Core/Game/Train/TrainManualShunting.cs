using RailDispatchMono.Core.Game.Simulation;
using System;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    private const float ManualShuntingAccelerationFactor = 1f;

    public void UpdateManualShunting(float deltaTime, float targetSpeedKmh)
    {
        if (deltaTime <= 0f || !CanMove || _map is null)
            return;

        float targetSpeedMps = MathF.Max(0f, targetSpeedKmh) / 3.6f;
        float locomotiveAcceleration = Composition.Locomotive?.Parameters.Acceleration ?? 0f;
        float accelerationRate = locomotiveAcceleration * GetMassPerformanceFactor() * ManualShuntingAccelerationFactor;

        Speed = MathF.Min(Speed + accelerationRate * deltaTime, targetSpeedMps);
        ClearRadioStop();

        float distance = SimulationScale.MetersToGrid(Speed * deltaTime);
        if (distance > MovementEpsilon)
            Move(distance);
    }
}
