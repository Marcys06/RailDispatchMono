using System;
using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Rendering;

public sealed class Camera
{
    public Vector2 Position { get; set; }

    public float Zoom { get; set; } = 32f;

    public Matrix Transform =>
        Matrix.CreateTranslation(
            -Position.X,
            -Position.Y,
            0f) *
        Matrix.CreateScale(Zoom);

    public void Move(Vector2 delta)
    {
        Position += delta;
    }
    public float Zoom { get; set; } = 32f;


    public void ZoomAt(
     Vector2 screenPoint,
     float delta)
    {
        var oldZoom = Zoom;

        Zoom = Math.Clamp(
            Zoom + delta,
            8f,
            64f);

        if (Math.Abs(Zoom - oldZoom) < 0.001f)
            return;

        var worldPosition =
            ScreenToWorld(screenPoint);

        Position =
            worldPosition -
            screenPoint / Zoom;
    }

    public Vector2 ScreenToWorld(
        Vector2 screen)
    {
        return
            screen / Zoom +
            Position;
    }

    public MapPosition ScreenToMap(
        Vector2 screen)
    {
        var world =
            ScreenToWorld(screen);

        return new MapPosition(
            (int)Math.Floor(world.X),
            (int)Math.Floor(world.Y));
    }
}