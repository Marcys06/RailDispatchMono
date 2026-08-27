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

    public void LoadContent(
        GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(
            graphicsDevice,
            1,
            1);

        _pixel.SetData(
            new[] { Color.White });
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
        const float thickness = 0.03f;

        for (int x = 0;
             x <= _map.Size.Width;
             x++)
        {
            DrawLine(
                spriteBatch,
                x,
                0,
                x,
                _map.Size.Height,
                Color.DarkGray,
                thickness);
        }

        for (int y = 0;
             y <= _map.Size.Height;
             y++)
        {
            DrawLine(
                spriteBatch,
                0,
                y,
                _map.Size.Width,
                y,
                Color.DarkGray,
                thickness);
        }
    }

    private void DrawTracks(
        SpriteBatch spriteBatch)
    {
        const float cellSize = 1f;
        const float thickness = 0.12f;

        foreach (var track in _map.Tracks.Values)
        {
            var x =
                track.Position.X *
                cellSize;

            var y =
                track.Position.Y *
                cellSize;

            var centerX =
                x + cellSize / 2f;

            var centerY =
                y + cellSize / 2f;

            var color =
                track.Geometry ==
                TrackGeometry.Curve
                    ? Color.Orange
                    : Color.Black;

            if (track.HasConnection(
                TrackConnections.North))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    centerX,
                    y,
                    color,
                    thickness);
            }

            if (track.HasConnection(
                TrackConnections.East))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    x + cellSize,
                    centerY,
                    color,
                    thickness);
            }

            if (track.HasConnection(
                TrackConnections.South))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    centerX,
                    y + cellSize,
                    color,
                    thickness);
            }

            if (track.HasConnection(
                TrackConnections.West))
            {
                DrawLine(
                    spriteBatch,
                    centerX,
                    centerY,
                    x,
                    centerY,
                    color,
                    thickness);
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
            (float)Math.Sqrt(
                dx * dx +
                dy * dy);

        var angle =
            (float)Math.Atan2(
                dy,
                dx);

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