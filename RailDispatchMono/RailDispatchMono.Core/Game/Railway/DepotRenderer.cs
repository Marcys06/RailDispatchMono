// DepotRenderer.cs
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Programmatic world-space renderer for depot buildings and placement preview.</summary>
public sealed class DepotRenderer
{
    private Texture2D? _pixel;
    private readonly DepotController _controller;

    public DepotRenderer(DepotController controller) => _controller = controller;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_pixel == null) return;

        foreach (var depot in _controller.Depots)
            DrawBuilding(spriteBatch, depot.Position, false);
    }

    public void DrawPreview(SpriteBatch spriteBatch, MapPosition position)
    {
        DrawBuilding(spriteBatch, position, true);
    }

    private void DrawBuilding(SpriteBatch spriteBatch, MapPosition position, bool preview)
    {
        if (_pixel == null) return;

        const float width = 0.9f;
        const float height = 0.9f;
        const float roofInset = 0.12f;
        const float doorWidth = 0.22f;
        const float doorHeight = 0.34f;
        const float roofThickness = 0.08f;

        Vector2 center = new(position.X + 0.5f, position.Y + 0.5f);
        Vector2 topLeft = center - new Vector2(width, height) * 0.5f;
        Vector2 bottomRight = center + new Vector2(width, height) * 0.5f;

        Color building = preview ? Color.SlateGray * 0.55f : new Color(75, 75, 85, 235);
        Color detail = preview ? Color.White * 0.65f : Color.White;

        DrawFilledRect(spriteBatch, center, new Vector2(width, height), building);
        DrawLine(spriteBatch,
            new Vector2(topLeft.X, topLeft.Y),
            new Vector2(bottomRight.X, topLeft.Y),
            detail, roofThickness);
        DrawLine(spriteBatch,
            new Vector2(bottomRight.X, topLeft.Y),
            new Vector2(bottomRight.X, bottomRight.Y),
            detail, roofThickness);
        DrawLine(spriteBatch,
            new Vector2(bottomRight.X, bottomRight.Y),
            new Vector2(topLeft.X, bottomRight.Y),
            detail, roofThickness);
        DrawLine(spriteBatch,
            new Vector2(topLeft.X, bottomRight.Y),
            new Vector2(topLeft.X, topLeft.Y),
            detail, roofThickness);

        DrawLine(spriteBatch,
            new Vector2(topLeft.X + roofInset, topLeft.Y + roofInset),
            new Vector2(bottomRight.X - roofInset, topLeft.Y + roofInset),
            detail, roofThickness);

        DrawLine(spriteBatch,
            new Vector2(center.X, topLeft.Y + roofInset),
            new Vector2(center.X, bottomRight.Y - roofInset),
            detail, 0.07f);

        DrawLine(spriteBatch,
            new Vector2(center.X - doorWidth * 0.5f, bottomRight.Y - doorHeight),
            new Vector2(center.X - doorWidth * 0.5f, bottomRight.Y),
            detail, 0.06f);
        DrawLine(spriteBatch,
            new Vector2(center.X + doorWidth * 0.5f, bottomRight.Y - doorHeight),
            new Vector2(center.X + doorWidth * 0.5f, bottomRight.Y),
            detail, 0.06f);
    }

    private void DrawFilledRect(SpriteBatch spriteBatch, Vector2 center, Vector2 size, Color color)
    {
        if (_pixel == null) return;
        spriteBatch.Draw(_pixel, center, null, color, 0f, new Vector2(0.5f), size, SpriteEffects.None, 0f);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        if (_pixel == null) return;
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0f) return;

        spriteBatch.Draw(
            _pixel,
            start,
            null,
            color,
            (float)Math.Atan2(delta.Y, delta.X),
            new Vector2(0f, 0.5f),
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f);
    }
}