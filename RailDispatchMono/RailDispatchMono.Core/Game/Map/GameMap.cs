#nullable enable
using RailDispatchMono.Core.Game.Railway;
using System.Collections.Generic;
using System;

namespace RailDispatchMono.Core.Game.Map;

public sealed class GameMap
{
    private readonly TerrainType[] _terrain;
    private readonly Dictionary<MapPosition, TrackCell> _tracks = new();

    public MapSize Size { get; }

    public IReadOnlyDictionary<MapPosition, TrackCell> Tracks =>
        _tracks;

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

    // ============================================================
    // TERRAIN
    // ============================================================

    public TerrainType GetTerrain(MapPosition position)
    {
        ValidatePosition(position);
        return _terrain[GetIndex(position)];
    }

    public void SetTerrain(MapPosition position, TerrainType terrain)
    {
        ValidatePosition(position);
        _terrain[GetIndex(position)] = terrain;
    }

    // ============================================================
    // TRACK MANAGEMENT
    // ============================================================

    public bool HasTrack(MapPosition position)
    {
        return _tracks.ContainsKey(position);
    }

    public bool TryGetTrack(MapPosition position, out TrackCell? track)
    {
        return _tracks.TryGetValue(position, out track);
    }

    public TrackCell? GetTrackAt(MapPosition position)
    {
        if (!TryGetTrack(position, out var track))
            return null;
        return track;
    }

    public void AddTrack(TrackCell track)
    {
        ValidatePosition(track.Position);
        _tracks[track.Position] = track;
    }

    public bool RemoveTrack(MapPosition position)
    {
        return _tracks.Remove(position);
    }


    public bool IsJunctionAt(MapPosition position)
    {
        return TryGetTrack(position, out var track) &&
               track != null &&
               track.Geometry == TrackGeometry.Junction;
    }

    public TrackCell? GetJunctionAt(MapPosition position)
    {
        return TryGetTrack(position, out var track) &&
               track?.Geometry == TrackGeometry.Junction ? track : null;
    }

    // GameMap.cs - dodaj brakuj¹ce metody
    public bool HasTrackAt(MapPosition position)
    {
        return _tracks.ContainsKey(position);
    }

    public IEnumerable<KeyValuePair<MapPosition, TrackCell>> GetAllTracks()
    {
        return _tracks;
    }

    public int TrackCount => _tracks.Count;

    public void Clear()
    {
        _tracks.Clear();
    }

    public bool IsInside(MapPosition position)
    {
        return position.X >= 0 && position.X < Size.Width &&
               position.Y >= 0 && position.Y < Size.Height;
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < Size.Width &&
               y >= 0 && y < Size.Height;
    }

    private int GetIndex(MapPosition position)
    {
        return checked(
            position.Y * Size.Width +
            position.X);
    }

    private void ValidatePosition(MapPosition position)
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