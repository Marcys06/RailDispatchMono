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
    private bool _consumeOpeningUpdate;

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

        // The menu is opened by the same input update that may still contain
        // the opening click. Consume that first update explicitly so it can
        // never be interpreted as a menu click/outside click.
        _previousMouse = Mouse.GetState();
        _consumeOpeningUpdate = true;
        ClampToViewport();
    }

    public void Close()
    {
        _wagon = null;
        _stations = null;
        _consumeOpeningUpdate = false;
    }

    private void Changed() => RouteChanged?.Invoke(_wagon!);

    public void Update(MouseState mouse)
    {
        if (!IsOpen || _wagon == null || _stations == null) return;

        if (_consumeOpeningUpdate)
        {
            _previousMouse = mouse;
            _consumeOpeningUpdate = false;
            return;
        }

        bool leftPressed = mouse.LeftButton == ButtonState.Pressed &&
                           _previousMouse.LeftButton == ButtonState.Released;
        bool rightPressed = mouse.RightButton == ButtonState.Pressed &&
                            _previousMouse.RightButton == ButtonState.Released;

        // PPM always closes the menu, independently of the cursor position.
        if (rightPressed)
        {
            Close();
            return;
        }

        // Only a NEW LPM click is considered. Holding LPM cannot repeatedly
        // trigger actions or close the menu.
        if (leftPressed)
        {
            if (ButtonRect(0, 0, 240, 34).Contains(mouse.Position))
            {
                Close();
                return;
            }

            int y = 44;

            // Each station button is handled independently. Once one button
            // matches, finish this update instead of falling through to the
            // remaining station/button ranges.
            foreach (var stationId in _wagon.Route.StationIds.ToList())
            {
                if (ButtonRect(0, y, 240, 30).Contains(mouse.Position))
                {
                    if (_wagon.Route.RemoveStation(stationId))
                        Changed();
                    _previousMouse = mouse;
                    return;
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
                    _previousMouse = mouse;
                    return;
                }
                y += 34;
            }

            if (ButtonRect(0, y + 6, 240, 30).Contains(mouse.Position))
            {
                _wagon.Route.Clear();
                Changed();
                _previousMouse = mouse;
                return;
            }

            // A new LPM outside the menu closes it. The opening click was
            // already consumed above, so this can only happen on a later click.
            var menuRect = new Rectangle((int)_position.X, (int)_position.Y, 250, (int)GetHeight());
            if (!menuRect.Contains(mouse.Position))
            {
                Close();
                return;
            }
        }

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

        // Active S marker: visual indication that the wagon-route edit mode
        // is currently active. It is intentionally not used as a menu action.
        var sButton = new Rectangle((int)_position.X + 218, (int)_position.Y + 7, 25, 25);
        spriteBatch.Draw(_pixel, sButton, Color.Yellow);
        Vector2 sSize = _font.MeasureString("S") * 0.65f;
        spriteBatch.DrawString(
            _font,
            "S",
            new Vector2(sButton.Center.X, sButton.Center.Y) - sSize / 2f,
            Color.Black,
            0f,
            Vector2.Zero,
            0.65f,
            SpriteEffects.None,
            0f);

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
