namespace RailDispatchMono.Core.Game.Train;

public sealed partial class Train
{
    /// <summary>Current infrastructure/signal speed target before the consist Vmax cap.</summary>
    public float TargetSpeed => _targetSpeed;

    /// <summary>Maximum speed permitted by the slowest vehicle in the consist.</summary>
    public float MaxSpeed => _maxSpeed;

    /// <summary>Effective target speed after applying the consist Vmax limit.</summary>
    public float EffectiveTargetSpeed => System.MathF.Min(_targetSpeed, _maxSpeed);
}
