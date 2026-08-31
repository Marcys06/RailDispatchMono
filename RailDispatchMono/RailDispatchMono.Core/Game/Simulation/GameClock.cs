using System;

namespace RailDispatchMono.Core.Game.Simulation;

/// <summary>
/// Lightweight 24-hour simulation clock. The multiplier affects simulation time,
/// while Pause is controlled by the existing screen pause system.
/// </summary>
public sealed class GameClock
{
    public const float SecondsPerDay = 24f * 60f * 60f;

    private float _seconds;

    public float SimulationSpeed { get; private set; } = 1f;
    public float Seconds => _seconds;
    public TimeSpan Time => TimeSpan.FromSeconds(_seconds);
    public string DisplayTime => $"{Time.Hours:00}:{Time.Minutes:00}";

    public void Reset() => _seconds = 0f;

    public void SetSpeed(float multiplier)
    {
        SimulationSpeed = multiplier switch
        {
            1f => 1f,
            2f => 2f,
            5f => 5f,
            _ => SimulationSpeed
        };
    }

    public float Update(float realDeltaSeconds)
    {
        if (realDeltaSeconds <= 0f) return 0f;
        float scaled = realDeltaSeconds * SimulationSpeed;
        _seconds = (_seconds + scaled) % SecondsPerDay;
        return scaled;
    }
}
