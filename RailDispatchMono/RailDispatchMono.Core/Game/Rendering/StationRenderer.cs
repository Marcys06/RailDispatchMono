using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.Game.Rendering;

/// <summary>
/// Renders station markers in world space. Text/name editing is intentionally
/// separate from the placement renderer and can be added later.
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
        {
            Vector2 center = new(station.Position.X + 0.5f, station.Position.Y + 0.5f);
            float size = 0.30f;
            float half = size / 2f;
            spriteBatch.Draw(_pixel, new Rectangle((int)(center.X - half), (int)(center.Y - half), Math.Max(1, (int)size), Math.Max(1, (int)size)), Color.CornflowerBlue);
            float post = 0.32f;
            DrawLine(spriteBatch, center + new Vector2(0f, -half), center + new Vector2(0f, -post), Color.White, 0.05f);
            DrawLine(spriteBatch, center + new Vector2(-0.18f, -post), center + new Vector2(0.18f, -post), Color.White, 0.05f);
        }
    }

    public void DrawPreview(SpriteBatch spriteBatch, MapPosition position)
    {
        if (_pixel == null) return;
        Vector2 center = new(position.X + 0.5f, position.Y + 0.5f);
        DrawLine(spriteBatch, center + new Vector2(-0.30f, 0f), center + new Vector2(0.30f, 0f), Color.CornflowerBlue * 0.65f, 0.12f);
        DrawLine(spriteBatch, center + new Vector2(0f, -0.30f), center + new Vector2(0f, 0.30f), Color.CornflowerBlue * 0.65f, 0.12f);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        if (_pixel == null) return;
        Vector2 delta = end - start;
        float length = delta.Length();
        if (length <= 0f) return;
        spriteBatch.Draw(_pixel, start, null, color, MathF.Atan2(delta.Y, delta.X), new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
    }
}
