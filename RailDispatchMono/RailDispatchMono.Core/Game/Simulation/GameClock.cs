using System;

namespace RailDispatchMono.Core.Game.Simulation;

/// <summary>
/// 24-hour simulation clock. Game time is intentionally faster than wall-clock time:
/// 5 seconds of game time pass during 1 second of real time at x1.
/// SimulationSpeed (x1/x2/x5) multiplies both game-clock progression and the normal
/// simulation delta used by systems such as trains. The fixed 5x base scale belongs
/// only to the clock representation and never changes physical speed or distance.
/// </summary>
public sealed class GameClock
{
    public const double SecondsPerDay = 24d * 60d * 60d;
    public const double BaseTimeScale = 5d;

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
        if (realDeltaSeconds <= 0f) return 0f;

        // Game clock: 5 seconds of game time per real second at x1.
        double clockDelta = realDeltaSeconds * BaseTimeScale * SimulationSpeed;
        _seconds = (_seconds + clockDelta) % SecondsPerDay;

        // Simulation delta deliberately excludes BaseTimeScale.
        // Train speed and travelled distance therefore remain unchanged at x1;
        // x2/x5 still accelerate the normal simulation as intended.
        return realDeltaSeconds * SimulationSpeed;
    }
}
