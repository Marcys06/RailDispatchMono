namespace RailDispatchMono.Core.Game.Simulation;

/// <summary>
/// Central physical scale used by gameplay systems that convert between map cells
/// and real-world metres. Rendering keeps the same grid geometry; only the physical
/// interpretation of a cell changes.
/// </summary>
public static class SimulationScale
{
    /// <summary>Real-world distance represented by one map cell.</summary>
    public const float MetersPerGridCell = 10f;

    public static float MetersToGrid(float meters) => meters / MetersPerGridCell;
    public static float GridToMeters(float cells) => cells * MetersPerGridCell;
}
