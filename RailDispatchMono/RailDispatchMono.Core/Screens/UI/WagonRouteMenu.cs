using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Linq;

namespace RailDispatchMono.Core.Screens.UI;

/// <summary>Lightweight screen-space editor for one wagon's station route.</summary>
public sealed class WagonRouteMenu
{
    private readonly GraphicsDevice _graphicsDevice;
    private SpriteFont? _font;
    private Texture2D? _pixel;
    private Wagon? _wagon;
    private StationController? _stations;
    private Vector2 _position;
    private MouseState _previousMouse;

    public bool IsOpen => _wagon != null && _stations != null;
    public event Action<Wagon>? RouteChanged;

    public WagonRouteMenu(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _previousMouse = Mouse.GetState();
    }

    public void SetFont(SpriteFont font) => _font = font;

    public void LoadContent()
    {
        if (_pixel != null) return;
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Open(Vector2 screenPosition, Wagon wagon, StationController stations)
    {
        _wagon = wagon;
        _stations = stations;
        _position = screenPosition + new Vector2(12f, 12f);
        _previousMouse = Mouse.GetState();
        ClampToViewport();
    }

    public void Close()
    {
        _wagon = null;
        _stations = null;
    }

    private void Changed() => RouteChanged?.Invoke(_wagon!);

    public void Update(MouseState mouse)
    {
        if (!IsOpen || _wagon == null || _stations == null) return;

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            if (ButtonRect(0, 0, 240, 34).Contains(mouse.Position))
            {
                Close();
                _previousMouse = mouse;
                return;
            }

            int y = 44;
            foreach (var stationId in _wagon.Route.StationIds.ToList())
            {
                if (ButtonRect(0, y, 240, 30).Contains(mouse.Position))
                {
                    if (_wagon.Route.RemoveStation(stationId)) Changed();
                    break;
                }
                y += 34;
            }

            y += 6;
            foreach (var station in _stations.Stations)
            {
                if (_wagon.Route.ServesStation(station.Id)) continue;
                if (ButtonRect(0, y, 240, 30).Contains(mouse.Position))
                {
                    _wagon.Route.AddStation(station.Id);
                    Changed();
                    break;
                }
                y += 34;
            }

            if (ButtonRect(0, y + 6, 240, 30).Contains(mouse.Position))
            {
                if (!_wagon.Route.IsEmpty) Changed();
                _wagon.Route.Clear();
            }
        }

        if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
        {
            Close();
            _previousMouse = mouse;
            return;
        }

        // The menu is opened at the wagon's click position. Do not use a
        // negative margin here: the old boundary closed the menu on the next
        // frame because the cursor was still on the original wagon cell.
        var menuRect = new Rectangle((int)_position.X, (int)_position.Y, 250, (int)GetHeight());
        if (mouse.LeftButton == ButtonState.Pressed && !menuRect.Contains(mouse.Position))
            Close();

        _previousMouse = mouse;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen || _wagon == null || _stations == null || _font == null || _pixel == null) return;

        ClampToViewport();
        float width = 250f, height = GetHeight();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        spriteBatch.Draw(_pixel, new Rectangle((int)_position.X, (int)_position.Y, (int)width, (int)height), new Color(18, 18, 18, 245));
        spriteBatch.Draw(_pixel, new Rectangle((int)_position.X, (int)_position.Y, (int)width, 3), Color.Yellow);
        DrawString(spriteBatch, "TRASA WAGONU", 8, 8, Color.Yellow, 0.75f);
        DrawString(spriteBatch, $"Przystanki: {_wagon.Route.StationIds.Count}", 8, 28, Color.White, 0.6f);

        int y = 44, number = 1;
        foreach (var stationId in _wagon.Route.StationIds)
        {
            var station = _stations.Stations.FirstOrDefault(s => s.Id == stationId);
            string name = station?.Name ?? $"Brak stacji ({stationId.ToString()[..8]})";
            string marker = stationId == _wagon.Route.CurrentStationId ? "*" : $"{number}.";
            DrawButton(spriteBatch, new Rectangle((int)_position.X + 5, (int)_position.Y + y, 240, 30), $"{marker} {name}  [USUŃ]", false);
            y += 34;
            number++;
        }

        y += 6;
        foreach (var station in _stations.Stations)
        {
            if (_wagon.Route.ServesStation(station.Id)) continue;
            DrawButton(spriteBatch, new Rectangle((int)_position.X + 5, (int)_position.Y + y, 240, 30), $"+ {station.Name}", false);
            y += 34;
        }

        DrawButton(spriteBatch, new Rectangle((int)_position.X + 5, (int)_position.Y + y + 6, 240, 30), "WYCZYŚĆ TRASĘ", false);
        spriteBatch.End();
    }

    private float GetHeight()
    {
        if (_wagon == null || _stations == null) return 160f;
        int addable = _stations.Stations.Count(s => !_wagon.Route.ServesStation(s.Id));
        return 82f + (_wagon.Route.StationIds.Count + addable) * 34f + 36f;
    }

    private Rectangle ButtonRect(int x, int y, int width, int height) =>
        new((int)_position.X + 5 + x, (int)_position.Y + y, width, height);

    private void DrawButton(SpriteBatch batch, Rectangle rect, string text, bool selected)
    {
        batch.Draw(_pixel!, rect, selected ? new Color(70, 90, 35, 245) : new Color(45, 45, 45, 245));
        float scale = 0.55f;
        Vector2 size = _font!.MeasureString(text) * scale;
        Vector2 p = new(rect.X + 6, rect.Y + (rect.Height - size.Y) / 2f);
        batch.DrawString(_font, text, p, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawString(SpriteBatch batch, string text, int x, int y, Color color, float scale) =>
        batch.DrawString(_font!, text, _position + new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private void ClampToViewport()
    {
        if (_font == null || _graphicsDevice.Viewport.Width <= 0) return;
        float width = 250f, height = GetHeight();
        _position.X = Microsoft.Xna.Framework.MathHelper.Clamp(_position.X, 4f, Math.Max(4f, _graphicsDevice.Viewport.Width - width - 4f));
        _position.Y = Microsoft.Xna.Framework.MathHelper.Clamp(_position.Y, 4f, Math.Max(4f, _graphicsDevice.Viewport.Height - height - 4f));
    }
}
