#nullable enable
using RailDispatchMono.Core.Game.Railway;
using System.Collections.Generic;
using System;

namespace RailDispatchMono.Core.Game.Map;

public sealed class GameMap
{
    private readonly TerrainType[] _terrain;
    private readonly Dictionary<MapPosition, TrackCell> _tracks = new();
    private readonly Dictionary<MapPosition, Junction> _junctions = new();

    public MapSize Size { get; }

    public IReadOnlyDictionary<MapPosition, TrackCell> Tracks =>
        _tracks;

    public IReadOnlyDictionary<MapPosition, Junction> Junctions =>
        _junctions;

    public GameMap(int width, int height)
        : this(new MapSize(width, height))
    {
    }

    public GameMap(MapSize size)
    {
        Size = size;

        _terrain = new TerrainType[
            checked(size.Width * size.Height)];
    }

    public TerrainType GetTerrain(
        MapPosition position)
    {
        ValidatePosition(position);

        return _terrain[GetIndex(position)];
    }

    public void SetTerrain(
        MapPosition position,
        TerrainType terrain)
    {
        ValidatePosition(position);

        _terrain[GetIndex(position)] = terrain;
    }

    public bool HasTrack(
        MapPosition position)
    {
        return _tracks.ContainsKey(position);
    }

    public bool TryGetTrack(
        MapPosition position,
        out TrackCell? track)
    {
        return _tracks.TryGetValue(
            position,
            out track);
    }

    public void AddTrack(
    TrackCell track)
    {
        ValidatePosition(track.Position);

        _junctions.Remove(track.Position);
        _tracks[track.Position] = track;
    }

    public bool RemoveTrack(
 MapPosition position)
    {
        var removedTrack = _tracks.Remove(position);
        var removedJunction = _junctions.Remove(position);

        return removedTrack || removedJunction;
    }

    public bool HasJunction(
        MapPosition position)
    {
        return _junctions.ContainsKey(position);
    }

    public bool TryGetJunction(
        MapPosition position,
        out Junction? junction)
    {
        return _junctions.TryGetValue(
            position,
            out junction);
    }

    public void AddJunction(
        Junction junction)
    {
        ValidatePosition(junction.Position);

        _tracks.Remove(junction.Position);
        _junctions[junction.Position] = junction;
    }

    



    private int GetIndex(
        MapPosition position)
    {
        return checked(
            position.Y * Size.Width +
            position.X);
    }

    private void ValidatePosition(
        MapPosition position)
    {
        if (position.X < 0 ||
            position.X >= Size.Width ||
            position.Y < 0 ||
            position.Y >= Size.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(position));
        }
    }
}










