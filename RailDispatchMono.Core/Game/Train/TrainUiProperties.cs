using System;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    /// <summary>Current speed target calculated from the next applicable signal.</summary>
    public float TargetSpeed => GetSpeedFromSignal(GetNextSignal());

    /// <summary>Maximum speed permitted by the locomotive power/load model and wagon limits.</summary>
    public float MaxSpeed => Composition.EffectiveMaxSpeed;

    /// <summary>Effective target speed after applying the consist Vmax limit.</summary>
    public float EffectiveTargetSpeed => MathF.Min(TargetSpeed, MaxSpeed);
}
