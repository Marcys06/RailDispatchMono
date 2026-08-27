using System;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Train;

public sealed class Train
{
    public Guid Id { get; }

    public TrainComposition Composition { get; }

    public Vector2 Position { get; set; }

    public float Speed { get; set; }

    private GameMap? _map;

    public Train()
    {
        Id = Guid.NewGuid();

        Composition =
            new TrainComposition();

        Position =
            new Vector2(
                2.5f,
                2.5f);

        Speed = 2.0f;
    }

    public bool CanMove =>
        Composition.CanMove;

    public float Length =>
        Composition.Length;

    public void SetMap(
        GameMap map)
    {
        _map = map;
    }

    public void Update(
        float deltaTime)
    {
        if (!CanMove ||
            _map is null)
        {
            return;
        }

        var mapPosition =
            new MapPosition(
                (int)MathF.Floor(Position.X),
                (int)MathF.Floor(Position.Y));

        if (!_map.TryGetTrack(
                mapPosition,
                out var track) ||
            track is null)
        {
            return;
        }

        if (!track.HasConnection(
                TrackConnections.East))
        {
            return;
        }

        Position +=
            Vector2.UnitX *
            Speed *
            deltaTime;
    }
}
