using System;

namespace RailDispatchMono.Core.Game.Simulation;

/// <summary>
/// Lightweight 24-hour simulation clock. The multiplier affects simulation time,
/// while Pause is controlled by the existing screen pause system.
/// </summary>
public sealed class GameClock
{
    public const double SecondsPerDay = 24d * 60d * 60d;

    private double _seconds;

    public float SimulationSpeed { get; private set; } = 1f;
    public double Seconds => _seconds;
    public TimeSpan Time => TimeSpan.FromSeconds(_seconds);
    public string DisplayTime => $"{Time.Hours:00}:{Time.Minutes:00}";

    public void Reset() => _seconds = 0d;

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
        if (realDeltaSeconds <= 0f)
            return 0f;

        double scaled = realDeltaSeconds * SimulationSpeed;
        _seconds = (_seconds + scaled) % SecondsPerDay;
        return (float)scaled;
    }
}
