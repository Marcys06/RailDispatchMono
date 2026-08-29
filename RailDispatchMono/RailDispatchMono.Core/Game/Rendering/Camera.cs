using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Map;
using System;

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

    public void ZoomAt(
        Vector2 screenPosition,
        float delta)
    {
        var worldBefore = ScreenToWorld(screenPosition);

        Zoom = Math.Clamp(
            Zoom + delta,
            8f,
            128f);

        var worldAfter = ScreenToWorld(screenPosition);

        Position += worldBefore - worldAfter;
    }

    public Vector2 ScreenToWorld(
        Vector2 screenPosition)
    {
        return screenPosition / Zoom + Position;
    }

    public MapPosition ScreenToMap(
        Vector2 screenPosition)
    {
        var world = ScreenToWorld(screenPosition);
        return new MapPosition(
            (int)Math.Floor(world.X),
            (int)Math.Floor(world.Y));
    }

    // ============================================================
    // DODANA METODA - WEWNĄTRZ KLASY, PRZED OSTATNIM NAWIASEM
    // ============================================================
    public Vector2 MapToScreen(Vector2 mapPosition)
    {
        return (mapPosition - Position) * Zoom;
    }
}