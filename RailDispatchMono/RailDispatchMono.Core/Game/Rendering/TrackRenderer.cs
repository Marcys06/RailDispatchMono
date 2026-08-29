using System;
using Debug = System.Diagnostics.Debug; // Alias wskazuj¹cy bezpoœrednio na systemowy Debug
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
namespace RailDispatchMono.Core.Game.Rendering;

public sealed class TrackRenderer
{
    private readonly GameMap _map;
    private Texture2D? _pixel;
    private SignalRenderer? _signalRenderer;

    public TrackRenderer(GameMap map)
    {
        _map = map;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        System.Diagnostics.Debug.WriteLine("[TRACK_RENDERER] LoadContent - pixel texture created");// $2
    }

    public void SetSignalRenderer(SignalRenderer signalRenderer)
    {
        _signalRenderer = signalRenderer;
        System.Diagnostics.Debug.WriteLine("[TRACK_RENDERER] SignalRenderer set");// $2
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (_pixel is null)
            return;

        DrawGrid(spriteBatch);
        DrawTracks(spriteBatch);
    }

    public void DrawPreview(
        SpriteBatch spriteBatch,
        MapPosition position,
        TrackBuildMode mode,
        bool straightHorizontal,
        CurveDirection curveDirection,
        JunctionType junctionType = JunctionType.South_NorthEast)
    {
        if (_pixel is null)
            return;

        if (position.X < 0 || position.X >= _map.Size.Width ||
            position.Y < 0 || position.Y >= _map.Size.Height)
        {
            return;
        }

        var cellPosition = new Vector2(position.X, position.Y);

        // TÅ‚o podglÄ…du (Å¼ÃlÅ‚te podÅ›wietlenie)
        spriteBatch.Draw(
            _pixel,
            cellPosition,
            null,
            Color.Yellow * 0.18f,
            0f,
            Vector2.Zero,
            Vector2.One,
            SpriteEffects.None,
            0f);

        TrackGeometry geometry;
        TrackConnections connections;

        if (mode == TrackBuildMode.Straight)
        {
            geometry = TrackGeometry.Straight;
            connections = straightHorizontal
                ? TrackConnections.West | TrackConnections.East
                : TrackConnections.North | TrackConnections.South;
        }
        else if (mode == TrackBuildMode.Curve)
        {
            geometry = TrackGeometry.Curve;
            connections = curveDirection switch
            {
                CurveDirection.NorthEast => TrackConnections.North | TrackConnections.East,
                CurveDirection.EastSouth => TrackConnections.East | TrackConnections.South,
                CurveDirection.SouthWest => TrackConnections.South | TrackConnections.West,
                CurveDirection.WestNorth => TrackConnections.West | TrackConnections.North,
                _ => TrackConnections.None
            };
        }
        else if (mode == TrackBuildMode.Junction)
        {
            geometry = TrackGeometry.Junction;
            connections = junctionType switch
            {
                JunctionType.South_NorthEast => TrackConnections.South | TrackConnections.North | TrackConnections.East,
                JunctionType.South_NorthWest => TrackConnections.South | TrackConnections.North | TrackConnections.West,
                JunctionType.West_EastSouth => TrackConnections.West | TrackConnections.East | TrackConnections.South,
                JunctionType.West_EastNorth => TrackConnections.West | TrackConnections.East | TrackConnections.North,
                _ => TrackConnections.None
            };
        }
        else if (mode == TrackBuildMode.Signal)
        {
            DrawSignalPreview(spriteBatch, cellPosition);
            return;
        }
        else
        {
            return;
        }

        DrawTrackLines(spriteBatch, position, geometry, connections, preview: true);
    }

    private void DrawSignalPreview(SpriteBatch spriteBatch, Vector2 cellPosition)
    {
        if (_pixel is null)
            return;

        Vector2 center = cellPosition + new Vector2(0.5f, 0.5f);
        float radius = 0.3f;
        float thickness = 0.04f;

        int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * MathHelper.TwoPi;
            float angle2 = ((i + 1) / (float)segments) * MathHelper.TwoPi;

            Vector2 p1 = center + new Vector2(MathF.Cos(angle1) * radius, MathF.Sin(angle1) * radius);
            Vector2 p2 = center + new Vector2(MathF.Cos(angle2) * radius, MathF.Sin(angle2) * radius);

            DrawLine(spriteBatch, p1, p2, Color.LimeGreen, thickness);
        }

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * MathHelper.TwoPi;
            float angle2 = ((i + 1) / (float)segments) * MathHelper.TwoPi;

            Vector2 p1 = center + new Vector2(MathF.Cos(angle1) * radius, MathF.Sin(angle1) * radius);
            Vector2 p2 = center + new Vector2(MathF.Cos(angle2) * radius, MathF.Sin(angle2) * radius);

            DrawTriangle(spriteBatch, center, p1, p2, Color.LimeGreen * 0.3f);
        }

        float sSize = 0.15f;
        Vector2 sPos = center - new Vector2(sSize * 0.3f, sSize * 0.6f);

        DrawLine(spriteBatch, sPos, sPos + new Vector2(sSize * 0.6f, 0), Color.White, thickness * 0.8f);
        DrawLine(spriteBatch, sPos + new Vector2(sSize * 0.6f, 0), sPos + new Vector2(sSize * 0.6f, sSize * 0.4f), Color.White, thickness * 0.8f);
        DrawLine(spriteBatch, sPos + new Vector2(sSize * 0.6f, sSize * 0.4f), sPos + new Vector2(0, sSize * 0.4f), Color.White, thickness * 0.8f);
        DrawLine(spriteBatch, sPos + new Vector2(0, sSize * 0.4f), sPos + new Vector2(0, sSize * 0.8f), Color.White, thickness * 0.8f);
        DrawLine(spriteBatch, sPos + new Vector2(0, sSize * 0.8f), sPos + new Vector2(sSize * 0.6f, sSize * 0.8f), Color.White, thickness * 0.8f);
        DrawLine(spriteBatch, sPos + new Vector2(sSize * 0.6f, sSize * 0.8f), sPos + new Vector2(sSize * 0.6f, sSize * 1.2f), Color.White, thickness * 0.8f);
    }

    private void DrawTriangle(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Vector2 p3, Color color)
    {
        if (_pixel is null)
            return;
        DrawLine(spriteBatch, p1, p2, color, 0.02f);
        DrawLine(spriteBatch, p2, p3, color, 0.02f);
        DrawLine(spriteBatch, p3, p1, color, 0.02f);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Color color, float thickness)
    {
        if (_pixel is null)
            return;

        Vector2 delta = p2 - p1;
        float length = delta.Length();

        if (length < 0.0001f)
            return;

        float angle = MathF.Atan2(delta.Y, delta.X);

        spriteBatch.Draw(
            _pixel,
            p1,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f);
    }

    private void DrawGrid(SpriteBatch spriteBatch)
    {
        const float gridThickness = 0.025f;

        for (int x = 0; x <= _map.Size.Width; x++)
        {
            DrawLine(spriteBatch, x, 0, x, _map.Size.Height, Color.DarkGray, gridThickness);
        }

        for (int y = 0; y <= _map.Size.Height; y++)
        {
            DrawLine(spriteBatch, 0, y, _map.Size.Width, y, Color.DarkGray, gridThickness);
        }
    }

    private void DrawTracks(SpriteBatch spriteBatch)
    {
        foreach (var track in _map.Tracks.Values)
        {
            if (track.IsJunction)
            {
                DrawJunctionTrack(spriteBatch, track);
            }
            else
            {
                DrawTrackLines(spriteBatch, track.Position, track.Geometry, track.Connections, false);
            }
        }
    }

    private void DrawTrackLines(
        SpriteBatch spriteBatch,
        MapPosition position,
        TrackGeometry geometry,
        TrackConnections connections,
        bool preview)
    {
        var x = position.X;
        var y = position.Y;
        var centerX = x + 0.5f;
        var centerY = y + 0.5f;

        var color = geometry == TrackGeometry.Curve ? Color.Orange : Color.Black;
        if (preview) color *= 0.5f;

        if (connections.HasFlag(TrackConnections.North))
            DrawLine(spriteBatch, centerX, centerY, centerX, y, color, 0.12f);
        if (connections.HasFlag(TrackConnections.East))
            DrawLine(spriteBatch, centerX, centerY, x + 1f, centerY, color, 0.12f);
        if (connections.HasFlag(TrackConnections.South))
            DrawLine(spriteBatch, centerX, centerY, centerX, y + 1f, color, 0.12f);
        if (connections.HasFlag(TrackConnections.West))
            DrawLine(spriteBatch, centerX, centerY, x, centerY, color, 0.12f);
    }

    private void DrawJunctionTrack(SpriteBatch spriteBatch, TrackCell track)
    {
        DrawTrackLines(spriteBatch, track.Position, track.Geometry, track.Connections, false);

        var x = track.Position.X;
        var y = track.Position.Y;
        var centerX = x + 0.5f;
        var centerY = y + 0.5f;

        TrackConnections activeExit = track.IsSwitchedToDiverging ? track.DivergingSide : track.StraightSide;
        Color bladeColor = track.IsSwitchedToDiverging ? Color.Orange : Color.Lime;

        Vector2 exitOffset = GetDirectionVector(activeExit) * 0.45f;
        Vector2 bladeEnd = new Vector2(centerX, centerY) + exitOffset;

        DrawLine(spriteBatch, centerX, centerY, bladeEnd.X, bladeEnd.Y, bladeColor, 0.18f);
    }

    private static Vector2 GetDirectionVector(TrackConnections side) => side switch
    {
        TrackConnections.North => new Vector2(0f, -1f),
        TrackConnections.East => new Vector2(1f, 0f),
        TrackConnections.South => new Vector2(0f, 1f),
        TrackConnections.West => new Vector2(-1f, 0f),
        _ => Vector2.Zero
    };

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
        var length = MathF.Sqrt(dx * dx + dy * dy);

        if (length <= 0f)
            return;

        var angle = MathF.Atan2(dy, dx);

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
