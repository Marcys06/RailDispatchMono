using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.Game.Rendering;

/// <summary>
/// Renders stations in world space. Station placement is intentionally
/// independent from track rendering so station UI can evolve later.
/// </summary>
public sealed class StationRenderer
{
    private readonly StationController _stationController;
    private Texture2D? _pixel;
    private SpriteFont? _font;

    public StationRenderer(StationController stationController)
    {
        _stationController = stationController ?? throw new ArgumentNullException(nameof(stationController));
    }

    public void LoadContent(GraphicsDevice graphicsDevice, SpriteFont font)
    {
        _font = font ?? throw new ArgumentNullException(nameof(font));
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (_pixel == null || _font == null) return;

        foreach (var station in _stationController.Stations)
        {
            Vector2 center = new(station.Position.X + 0.5f, station.Position.Y + 0.5f);
            Vector2 screen = Vector2.Transform(center, camera.Transform);
            float scale = MathF.Max(0.5f, camera.Zoom / 32f);
            float width = 0.65f * camera.Zoom;
            float height = 0.18f * camera.Zoom;
            var platform = new Rectangle((int)(screen.X - width / 2f), (int)(screen.Y - height / 2f), Math.Max(4, (int)width), Math.Max(3, (int)height));
            spriteBatch.Draw(_pixel, platform, Color.White);

            string label = station.Name;
            Vector2 textSize = _font.MeasureString(label) * scale;
            Vector2 textPosition = new(screen.X - textSize.X / 2f, screen.Y - height / 2f - textSize.Y - 4f);
            spriteBatch.DrawString(_font, label, textPosition, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
