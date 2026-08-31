using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Map;

namespace RailDispatchMono.Core.Game.Railway;

/// <summary>Programmatic renderer for depot buildings and their placement preview.</summary>
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
        {
            var rect = new Rectangle(depot.Position.X * 32, depot.Position.Y * 32, 32, 32);
            spriteBatch.Draw(_pixel, rect, new Color(75, 75, 85, 235));
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + 4, rect.Y + 4, 24, 4), Color.White);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X + 14, rect.Y + 8, 4, 20), Color.White);
        }
    }

    public void DrawPreview(SpriteBatch spriteBatch, MapPosition position)
    {
        if (_pixel == null) return;
        var rect = new Rectangle(position.X * 32, position.Y * 32, 32, 32);
        spriteBatch.Draw(_pixel, rect, new Color(120, 120, 130, 120));
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 6, rect.Y + 14, 20, 4), Color.White * 0.7f);
        spriteBatch.Draw(_pixel, new Rectangle(rect.X + 14, rect.Y + 6, 4, 20), Color.White * 0.7f);
    }
}
