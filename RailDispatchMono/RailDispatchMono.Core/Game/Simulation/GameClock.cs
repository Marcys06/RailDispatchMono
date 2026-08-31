using System;

namespace RailDispatchMono.Core.Game.Simulation;

/// <summary>24-hour simulation clock with a persisted day counter.</summary>
public sealed class GameClock
{
    public const double SecondsPerDay = 24d * 60d * 60d;
    public const double BaseTimeScale = 5d;

    private double _seconds;

    public static GameClock? Current { get; private set; }
    public float SimulationSpeed { get; private set; } = 1f;
    public double Seconds => _seconds;
    public int GameDay { get; private set; } = 1;
    public TimeSpan Time => TimeSpan.FromSeconds(_seconds);
    public string DisplayTime => $"{Time.Hours:00}:{Time.Minutes:00}";

    public GameClock() => Current = this;

    public void Reset()
    {
        _seconds = 0d;
        GameDay = 1;
    }

    public void SetTime(int gameDay, double seconds)
    {
        GameDay = Math.Max(1, gameDay);
        _seconds = Math.Clamp(seconds, 0d, SecondsPerDay - double.Epsilon);
    }

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

        double clockDelta = realDeltaSeconds * BaseTimeScale * SimulationSpeed;
        _seconds += clockDelta;
        while (_seconds >= SecondsPerDay)
        {
            _seconds -= SecondsPerDay;
            GameDay++;
        }

        return realDeltaSeconds * SimulationSpeed;
    }
}
