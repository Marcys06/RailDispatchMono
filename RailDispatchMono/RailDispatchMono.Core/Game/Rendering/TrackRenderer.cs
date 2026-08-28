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

        if (position.X < 0 ||
            position.X >= _map.Size.Width ||
            position.Y < 0 ||
            position.Y >= _map.Size.Height)
        {
            return;
        }

        var cellPosition = new Vector2(position.X, position.Y);

        // Tło podglądu (żółte podświetlenie)
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
        else // TrackBuildMode.Junction (Klawisz 3)
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

        DrawTrackLines(
            spriteBatch,
            position,
            geometry,
            connections,
            preview: true);
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
            // Jeśli komórka jest zwrotnicą, używamy specjalnej logiki rysowania
            if (track.IsJunction)
            {
                DrawJunctionTrack(spriteBatch, track);
            }
            else
            {
                DrawTrackLines(
                    spriteBatch,
                    track.Position,
                    track.Geometry,
                    track.Connections,
                    false);
            }
        }
    }

    private void DrawTrack(
        SpriteBatch spriteBatch,
        MapPosition position,
        TrackGeometry geometry,
        TrackConnections connections,
        bool preview)
    {
        DrawTrackLines(spriteBatch, position, geometry, connections, preview);
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

        var color =
            geometry == TrackGeometry.Curve
                ? Color.Orange
                : Color.Black;

        if (preview)
        {
            color *= 0.5f;
        }

        if (connections.HasFlag(
                TrackConnections.North))
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

        if (connections.HasFlag(
                TrackConnections.East))
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

        if (connections.HasFlag(
                TrackConnections.South))
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

        if (connections.HasFlag(
                TrackConnections.West))
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

    // --- NOWA METODA: RYSOWANIE ZWROTNICY Z IGLICĄ ---
    private void DrawJunctionTrack(
        SpriteBatch spriteBatch,
        TrackCell track)
    {
        // 1. Rysuj całą bazę zwrotnicy (wszystkie połączone tory) w kolorze ciemno-szarym/czarnym
        DrawTrackLines(
            spriteBatch,
            track.Position,
            track.Geometry,
            track.Connections,
            false);

        // 2. Rysuj aktywną iglicę zwrotnicy
        var x = track.Position.X;
        var y = track.Position.Y;
        var centerX = x + 0.5f;
        var centerY = y + 0.5f;

        // Określamy dokąd kieruje iglica (wyjście wprost czy na skręt)
        TrackConnections activeExit = track.IsSwitchedToDiverging ? track.DivergingSide : track.StraightSide;

        // Kolor iglicy: Lime = Wprost (Straight), Orange = Skręt (Diverging)
        Color bladeColor = track.IsSwitchedToDiverging ? Color.Orange : Color.Lime;

        // Obliczamy wektor końca iglicy w stronę aktywnego wyjścia
        Vector2 exitOffset = GetDirectionVector(activeExit) * 0.45f;
        Vector2 bladeEnd = new Vector2(centerX, centerY) + exitOffset;

        // Rysujemy pogrubioną linię iglicy wskazującą aktywny tor
        DrawLine(
            spriteBatch,
            centerX,
            centerY,
            bladeEnd.X,
            bladeEnd.Y,
            bladeColor,
            0.18f);
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