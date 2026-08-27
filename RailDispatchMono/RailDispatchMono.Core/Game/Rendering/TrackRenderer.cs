using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Rendering;

public sealed class TrackRenderer
{
    private readonly GameMap _map;
    private Texture2D? _pixel;

    public TrackRenderer(GameMap map)
    {
        _map = map;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(
            graphicsDevice,
            1,
            1);

        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(
        SpriteBatch spriteBatch,
        Camera camera)
    {
        if (_pixel is null)
            return;

        DrawGrid(spriteBatch);
        DrawTracks(spriteBatch);
    }

    private void DrawGrid(
        SpriteBatch spriteBatch)
    {
        const float gridThickness = 0.025f;

        for (int x = 0; x <= _map.Size.Width; x++)
        {
            DrawLine(
                spriteBatch,
                x,
                0,
                x,
                _map.Size.Height,
                Color.DarkGray,
                gridThickness);
        }

        for (int y = 0; y <= _map.Size.Height; y++)
        {
            DrawLine(
                spriteBatch,
                0,
                y,
                _map.Size.Width,
                y,
                Color.DarkGray,
                gridThickness);
        }
    }

    private void DrawTracks(
        SpriteBatch spriteBatch)
    {
        foreach (var track in _map.Tracks.Values)
        {
            var x = track.Position.X;
            var y = track.Position.Y;

            var centerX = x + 0.5f;
            var centerY = y + 0.5f;

            var color =
                track.Geometry == TrackGeometry.Curve
                    ? Color.Orange
                    : Color.Black;

            if (track.HasConnection(TrackConnections.North))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    centerX,
                    y,
                    color,
                    0.12f);
            }

            if (track.HasConnection(TrackConnections.East))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    x + 1f,
                    centerY,
                    color,
                    0.12f);
            }

            if (track.HasConnection(TrackConnections.South))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    centerX,
                    y + 1f,
                    color,
                    0.12f);
            }

            if (track.HasConnection(TrackConnections.West))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    x,
                    centerY,
                    color,
                    0.12f);
            }
        }
    }

    private void DrawLine(
        SpriteBatch spriteBatch,
        float x1,
        float y1,
        float x2,
        float y2,
        Color color,
        float thickness)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;

        var length =
            MathF.Sqrt(dx * dx + dy * dy);

        if (length <= 0f)
            return;

        var angle =
            MathF.Atan2(dy, dx);

        spriteBatch.Draw(
            _pixel!,
            new Vector2(x1, y1),
            null,
            color,
            angle,
            new Vector2(0f, 0.5f),
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f);
    }
}
