using System;
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

    public TrackRenderer(GameMap map) => _map = map;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        DebugManager.Log("[TRACK_RENDERER] LoadContent - pixel texture created");
    }

    public void SetSignalRenderer(SignalRenderer signalRenderer) => _signalRenderer = signalRenderer;

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (_pixel is null) return;
        DrawGrid(spriteBatch);
        DrawTracks(spriteBatch);
    }

    public void DrawPreview(SpriteBatch spriteBatch, MapPosition position, TrackBuildMode mode, bool straightHorizontal, CurveDirection curveDirection, JunctionType junctionType = JunctionType.South_NorthEast)
    {
        if (_pixel is null || !IsInsideMap(position)) return;
        var cellPosition = new Vector2(position.X, position.Y);
        spriteBatch.Draw(_pixel, cellPosition, null, Color.Yellow * 0.18f, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
        TrackGeometry geometry;
        TrackConnections connections;
        if (mode == TrackBuildMode.Straight)
        {
            geometry = TrackGeometry.Straight;
            connections = straightHorizontal ? TrackConnections.West | TrackConnections.East : TrackConnections.North | TrackConnections.South;
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
            connections = GetJunctionConnections(junctionType);
        }
        else if (mode == TrackBuildMode.Signal) { DrawSignalPreview(spriteBatch, cellPosition); return; }
        else return;
        DrawTrackLines(spriteBatch, position, geometry, connections, true, TrackType.Mainline, LineClass.Mainline, TractionSystem.None);
    }

    private static TrackConnections GetJunctionConnections(JunctionType type) => type switch
    {
        JunctionType.South_NorthEast => TrackConnections.South | TrackConnections.North | TrackConnections.East,
        JunctionType.South_NorthWest => TrackConnections.South | TrackConnections.North | TrackConnections.West,
        JunctionType.South_EastWest => TrackConnections.South | TrackConnections.East | TrackConnections.West,
        JunctionType.North_SouthEast => TrackConnections.North | TrackConnections.South | TrackConnections.East,
        JunctionType.North_SouthWest => TrackConnections.North | TrackConnections.South | TrackConnections.West,
        JunctionType.North_EastWest => TrackConnections.North | TrackConnections.East | TrackConnections.West,
        JunctionType.East_WestNorth => TrackConnections.East | TrackConnections.West | TrackConnections.North,
        JunctionType.East_WestSouth => TrackConnections.East | TrackConnections.West | TrackConnections.South,
        JunctionType.East_NorthSouth => TrackConnections.East | TrackConnections.North | TrackConnections.South,
        JunctionType.West_EastNorth => TrackConnections.West | TrackConnections.East | TrackConnections.North,
        JunctionType.West_EastSouth => TrackConnections.West | TrackConnections.East | TrackConnections.South,
        JunctionType.West_NorthSouth => TrackConnections.West | TrackConnections.North | TrackConnections.South,
        _ => TrackConnections.None
    };

    private void DrawSignalPreview(SpriteBatch spriteBatch, Vector2 cellPosition)
    {
        Vector2 center = cellPosition + new Vector2(0.5f, 0.5f);
        float radius = 0.3f, thickness = 0.04f;
        const int segments = 20;
        for (int i = 0; i < segments; i++)
        {
            float a1 = i / (float)segments * MathHelper.TwoPi;
            float a2 = (i + 1) / (float)segments * MathHelper.TwoPi;
            Vector2 p1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;
            Vector2 p2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;
            DrawLine(spriteBatch, p1, p2, Color.LimeGreen, thickness);
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch)
    {
        const float gridThickness = 0.025f;
        for (int x = 0; x <= _map.Size.Width; x++) DrawLine(spriteBatch, x, 0, x, _map.Size.Height, Color.DarkGray, gridThickness);
        for (int y = 0; y <= _map.Size.Height; y++) DrawLine(spriteBatch, 0, y, _map.Size.Width, y, Color.DarkGray, gridThickness);
    }

    private void DrawTracks(SpriteBatch spriteBatch)
    {
        foreach (var track in _map.Tracks.Values)
            if (track.IsJunction) DrawJunctionTrack(spriteBatch, track);
            else DrawTrackLines(spriteBatch, track.Position, track.Geometry, track.Connections, false, track.Type, track.LineClass, track.Traction);
    }

    private void DrawTrackLines(SpriteBatch spriteBatch, MapPosition position, TrackGeometry geometry, TrackConnections connections, bool preview, TrackType type, LineClass lineClass, TractionSystem traction)
    {
        float x = position.X, y = position.Y, centerX = x + 0.5f, centerY = y + 0.5f;
        Color color = GetTractionColor(traction);
        float thickness = GetLineThickness(lineClass, type);
        if (preview) color *= 0.5f;
        if (connections.HasFlag(TrackConnections.North)) DrawLine(spriteBatch, centerX, centerY, centerX, y, color, thickness);
        if (connections.HasFlag(TrackConnections.East)) DrawLine(spriteBatch, centerX, centerY, x + 1f, centerY, color, thickness);
        if (connections.HasFlag(TrackConnections.South)) DrawLine(spriteBatch, centerX, centerY, centerX, y + 1f, color, thickness);
        if (connections.HasFlag(TrackConnections.West)) DrawLine(spriteBatch, centerX, centerY, x, centerY, color, thickness);
    }

    private void DrawJunctionTrack(SpriteBatch spriteBatch, TrackCell track)
    {
        DrawTrackLines(spriteBatch, track.Position, track.Geometry, track.Connections, false, track.Type, track.LineClass, track.Traction);
        float centerX = track.Position.X + 0.5f, centerY = track.Position.Y + 0.5f;
        TrackConnections activeExit = track.IsSwitchedToDiverging ? track.DivergingSide : track.StraightSide;
        Color bladeColor = track.IsSwitchedToDiverging ? Color.Orange : Color.Lime;
        Vector2 end = new Vector2(centerX, centerY) + GetDirectionVector(activeExit) * 0.45f;
        DrawLine(spriteBatch, centerX, centerY, end.X, end.Y, bladeColor, Math.Max(0.08f, GetLineThickness(track.LineClass, track.Type)));
    }

    private static Color GetTractionColor(TractionSystem traction) => traction switch
    {
        TractionSystem.AC => Color.CornflowerBlue,
        TractionSystem.DC => Color.OrangeRed,
        _ => Color.Black
    };

    private static float GetLineThickness(LineClass lineClass, TrackType type)
    {
        float thickness = lineClass switch
        {
            LineClass.Local => 0.08f,
            LineClass.Regional => 0.105f,
            LineClass.Mainline => 0.13f,
            LineClass.Magistral => 0.16f,
            _ => 0.1f
        };
        return type switch
        {
            TrackType.Siding => thickness * 0.78f,
            TrackType.Platform => thickness * 1.05f,
            TrackType.Secondary => thickness * 0.9f,
            _ => thickness
        };
    }

    private static Vector2 GetDirectionVector(TrackConnections side) => side switch
    {
        TrackConnections.North => new Vector2(0f, -1f),
        TrackConnections.East => new Vector2(1f, 0f),
        TrackConnections.South => new Vector2(0f, 1f),
        TrackConnections.West => new Vector2(-1f, 0f),
        _ => Vector2.Zero
    };

    private void DrawLine(SpriteBatch spriteBatch, Vector2 p1, Vector2 p2, Color color, float thickness)
    {
        if (_pixel is null) return;
        Vector2 delta = p2 - p1;
        float length = delta.Length();
        if (length < 0.0001f) return;
        float angle = MathF.Atan2(delta.Y, delta.X);
        spriteBatch.Draw(_pixel, p1, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    private void DrawLine(SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color, float thickness)
    {
        if (_pixel is null) return;
        float dx = x2 - x1, dy = y2 - y1, length = MathF.Sqrt(dx * dx + dy * dy);
        if (length <= 0f) return;
        float angle = MathF.Atan2(dy, dx);
        spriteBatch.Draw(_pixel, new Vector2(x1, y1), null, color, angle, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
    }

    private bool IsInsideMap(MapPosition position) => position.X >= 0 && position.X < _map.Size.Width && position.Y >= 0 && position.Y < _map.Size.Height;
}
