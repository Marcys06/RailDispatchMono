using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.Game.Rendering;

/// <summary>
/// Renders station areas in world space.
/// </summary>
public sealed class StationRenderer
{
    private readonly StationController _stationController;
    private Texture2D? _pixel;

    public StationRenderer(StationController stationController)
    {
        _stationController = stationController ?? throw new ArgumentNullException(nameof(stationController));
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_pixel == null) return;

        foreach (var station in _stationController.Stations)
            DrawArea(spriteBatch, station.Position, station.Width, station.Height, 0.12f);
    }

    public void DrawPreview(SpriteBatch spriteBatch, MapPosition position)
    {
        DrawPreview(spriteBatch, position, 1, 1);
    }

    public void DrawPreview(SpriteBatch spriteBatch, MapPosition position, int width, int height)
    {
        if (_pixel == null) return;
        width = Math.Max(1, width);
        height = Math.Max(1, height);

        // Keep the cursor on the center of the preview.
        var origin = new MapPosition(
            position.X - (width - 1) / 2,
            position.Y - (height - 1) / 2);

        DrawArea(spriteBatch, origin, width, height, 0.08f, true);
    }

    private void DrawArea(SpriteBatch spriteBatch, MapPosition origin, int width, int height, float fillAlpha, bool preview = false)
    {
        if (_pixel == null) return;

        float left = origin.X;
        float top = origin.Y;
        float right = origin.X + width;
        float bottom = origin.Y + height;
        Color color = preview ? Color.CornflowerBlue * 0.65f : Color.CornflowerBlue;

        // Area fill is intentionally subtle so rails, signals and trains remain visible.
        spriteBatch.Draw(_pixel,
            new Rectangle((int)left, (int)top, Math.Max(1, width), Math.Max(1, height)),
            color * fillAlpha);

        const float thickness = 0.08f;
        DrawLine(spriteBatch, new Vector2(left, top), new Vector2(right, top), color, thickness);
        DrawLine(spriteBatch, new Vector2(right, top), new Vector2(right, bottom), color, thickness);
        DrawLine(spriteBatch, new Vector2(right, bottom), new Vector2(left, bottom), color, thickness);
        DrawLine(spriteBatch, new Vector2(left, bottom), new Vector2(left, top), color, thickness);

        // Station marker in the center of the area.
        Vector2 center = new(origin.X + width / 2f, origin.Y + height / 2f);
        float marker = MathF.Min(0.30f, MathF.Min(width, height) * 0.18f);
        float half = marker / 2f;
        spriteBatch.Draw(_pixel,
            new Rectangle((int)(center.X - half), (int)(center.Y - half), Math.Max(1, (int)marker), Math.Max(1, (int)marker)),
            color);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        if (_pixel == null) return;
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0f) return;
        spriteBatch.Draw(_pixel, start, null, color, MathF.Atan2(delta.Y, delta.X),
            new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
    }
}
