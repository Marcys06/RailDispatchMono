using System;

namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    /// <summary>Current speed target calculated from the next applicable signal.</summary>
    public float TargetSpeed => GetSpeedFromSignal(GetNextSignal());

    /// <summary>Maximum speed permitted by the slowest vehicle in the consist.</summary>
    public float MaxSpeed => _maxSpeed;

    /// <summary>Effective target speed after applying the consist Vmax limit.</summary>
    public float EffectiveTargetSpeed => MathF.Min(TargetSpeed, MaxSpeed);

    /// <summary>Current braking capability after applying the non-linear consist-mass penalty.</summary>
    public float EffectiveBrakingRate => GetBrakingRate();
}
