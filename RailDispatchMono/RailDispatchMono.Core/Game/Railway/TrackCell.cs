using RailDispatchMono.Core.Game.Map;
using System.Collections.Generic;

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

    // W pliku TrackCell.cs
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
            // Dla zwykłego toru maskujemy i zwracamy przeciwne połączenie
            return Connections & ~entryDir;
        }

        // Wjazd od strony pnia (Ostrze zwrotnicy -> Rozjazd na dwa tory)
        // Używamy HasFlag, aby bezbłędnie sprawdzić dopasowanie bitowe
        if (CommonStem.HasFlag(entryDir) || entryDir.HasFlag(CommonStem))
        {
            return CurrentSwitchPosition == SwitchPosition.Straight
                ? StraightConnection
                : DivergingConnection;
        }

        // Wjazd od strony odgałęzień (Z rozpory do wspólnego pnia)
        return CommonStem;
    }
    public List<TrackConnections> GetAvailableDirections()
    {
        var result = new List<TrackConnections>();

        if (Connections.HasFlag(TrackConnections.North))
            result.Add(TrackConnections.North);
        if (Connections.HasFlag(TrackConnections.East))
            result.Add(TrackConnections.East);
        if (Connections.HasFlag(TrackConnections.South))
            result.Add(TrackConnections.South);
        if (Connections.HasFlag(TrackConnections.West))
            result.Add(TrackConnections.West);

        return result;
    }
}