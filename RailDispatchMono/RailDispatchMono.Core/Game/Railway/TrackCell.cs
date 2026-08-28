using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

public enum SwitchPosition
{
    Straight = 0,
    Diverging = 1
}

public sealed class TrackCell
{
    public MapPosition Position { get; }
    public TrackGeometry Geometry { get; private set; }
    public TrackConnections Connections { get; private set; }

    // --- STAN ZWROTNICY ---
    public SwitchPosition CurrentSwitchPosition { get; private set; } = SwitchPosition.Straight;

    // Połączenia zwrotnicy
    public TrackConnections StraightConnection { get; private set; } = TrackConnections.None;
    public TrackConnections DivergingConnection { get; private set; } = TrackConnections.None;
    public TrackConnections CommonStem { get; private set; } = TrackConnections.None;

    // --- WŁAŚCIWOŚCI POMOCNICZE (UŻYWANE M.IN. PRZEZ TRACKRENDERER) ---
    public bool IsJunction => Geometry == TrackGeometry.Junction;
    public bool IsSwitchedToDiverging => CurrentSwitchPosition == SwitchPosition.Diverging;
    public TrackConnections StraightSide => StraightConnection;
    public TrackConnections DivergingSide => DivergingConnection;
    public TrackConnections StemSide => CommonStem;

    public TrackCell(
        MapPosition position,
        TrackGeometry geometry,
        TrackConnections connections)
    {
        Position = position;
        Geometry = geometry;
        Connections = connections;
    }

    public void SetGeometry(TrackGeometry geometry)
    {
        Geometry = geometry;
    }

    public void SetConnections(TrackConnections connections)
    {
        Connections = connections;
    }

    public bool HasConnection(TrackConnections connection)
    {
        return Connections.HasFlag(connection);
    }

    // --- METODY INTERAKCJI Z ROZJAZDEM ---

    public void ConfigureJunction(
        TrackConnections commonStem,
        TrackConnections straightExit,
        TrackConnections divergingExit)
    {
        Geometry = TrackGeometry.Junction;
        CommonStem = commonStem;
        StraightConnection = straightExit;
        DivergingConnection = divergingExit;
        Connections = commonStem | straightExit | divergingExit;
    }

    public void ToggleSwitch()
    {
        if (Geometry != TrackGeometry.Junction)
            return;

        CurrentSwitchPosition = CurrentSwitchPosition == SwitchPosition.Straight
            ? SwitchPosition.Diverging
            : SwitchPosition.Straight;
    }

    /// <summary>
    /// Zwraca kierunek wyjścia z komórki na podstawie kierunku wjazdu pociągu (entryDir).
    /// </summary>
    public TrackConnections GetExitDirection(TrackConnections entryDir)
    {
        if (Geometry != TrackGeometry.Junction)
        {
            // Dla zwykłego toru zwracamy drugie wolne połączenie
            return Connections & ~entryDir;
        }

        // Jeśli wjeżdżamy od strony wspólnego pnia (CommonStem -> Ostrze zwrotnicy)
        if (entryDir == CommonStem)
        {
            return CurrentSwitchPosition == SwitchPosition.Straight
                ? StraightConnection
                : DivergingConnection;
        }

        // Jeśli wjeżdżamy od strony odgałęzień (Jazda z rozpory / do ostrza) -> wyjście jest zawsze w stronę CommonStem
        return CommonStem;
    }
}