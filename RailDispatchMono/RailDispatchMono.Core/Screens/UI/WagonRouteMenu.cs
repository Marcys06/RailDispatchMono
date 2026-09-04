using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RailDispatchMono.Core.Screens.UI;

/// <summary>
/// Existing S-menu, extended into the wagon timetable editor. The wagon owns
/// the timetable; the train is only the current operational grouping.
/// </summary>
public sealed class WagonRouteMenu
{
    private readonly GraphicsDevice _graphicsDevice;
    private SpriteFont? _font;
    private Texture2D? _pixel;
    private Wagon? _wagon;
    private StationController? _stations;
    private Vector2 _position;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private bool _consumeOpeningUpdate;
    private int _activeTimeField = -1;
    private bool _activeDeparture;
    private readonly List<string> _arrivals = new();
    private readonly List<string> _departures = new();
    private string _status = "";

    public bool IsOpen => _wagon != null && _stations != null;
    public event Action<Wagon>? RouteChanged;

    public WagonRouteMenu(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _previousMouse = Mouse.GetState();
        _previousKeyboard = Keyboard.GetState();
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
        _previousKeyboard = Keyboard.GetState();
        _consumeOpeningUpdate = true;
        _activeTimeField = -1;
        _status = "";

        _arrivals.Clear();
        _departures.Clear();
        var schedule = wagon.Schedule;
        if (schedule != null && schedule.BaseStationIds.Count >= 2)
        {
            foreach (var point in schedule.Points)
            {
                _arrivals.Add(FormatTime(point.ArrivalSeconds));
                _departures.Add(FormatTime(point.DepartureSeconds));
            }
        }
        else
        {
            var loop = wagon.Route.StationIds.Concat(wagon.Route.StationIds.Skip(1).Reverse()).ToList();
            EnsureTimeFields(loop.Count);
        }
        ClampToViewport();
    }

    public void Close()
    {
        _wagon = null;
        _stations = null;
        _consumeOpeningUpdate = false;
        _activeTimeField = -1;
    }

    private void Changed() => RouteChanged?.Invoke(_wagon!);

    public void Update(MouseState mouse)
    {
        if (!IsOpen || _wagon == null || _stations == null) return;

        if (_consumeOpeningUpdate)
        {
            _previousMouse = mouse;
            _previousKeyboard = Keyboard.GetState();
            _consumeOpeningUpdate = false;
            return;
        }

        KeyboardState keyboard = Keyboard.GetState();
        HandleKeyboard(keyboard);

        bool leftPressed = mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;
        bool rightPressed = mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released;
        if (rightPressed)
        {
            Close();
            return;
        }

        if (leftPressed)
        {
            if (ButtonRect(0, 0, 650, 34).Contains(mouse.Position))
            {
                Close();
                return;
            }

            var baseRoute = GetBaseRoute();
            if (ButtonRect(5, 44, 650, 30).Contains(mouse.Position))
            {
                SaveSchedule();
                _previousMouse = mouse;
                _previousKeyboard = keyboard;
                return;
            }

            int routeY = 84;
            for (int i = 0; i < baseRoute.Count; i++)
            {
                if (ButtonRect(5, routeY, 320, 28).Contains(mouse.Position))
                {
                    baseRoute.RemoveAt(i);
                    ApplyBaseRoute(baseRoute);
                    _previousMouse = mouse;
                    _previousKeyboard = keyboard;
                    return;
                }
                routeY += 31;
            }

            int addY = routeY + 4;
            foreach (var station in _stations.Stations)
            {
                if (baseRoute.Contains(station.Id)) continue;
                if (ButtonRect(5, addY, 320, 28).Contains(mouse.Position))
                {
                    baseRoute.Add(station.Id);
                    ApplyBaseRoute(baseRoute);
                    _previousMouse = mouse;
                    _previousKeyboard = keyboard;
                    return;
                }
                addY += 31;
            }

            var loop = baseRoute.Concat(baseRoute.Skip(1).Reverse()).ToList();
            int timeY = 84;
            for (int i = 0; i < loop.Count; i++)
            {
                if (TimeRect(335, timeY, 140, 28).Contains(mouse.Position))
                {
                    _activeTimeField = i;
                    _activeDeparture = false;
                    _previousMouse = mouse;
                    _previousKeyboard = keyboard;
                    return;
                }
                if (TimeRect(485, timeY, 140, 28).Contains(mouse.Position))
                {
                    _activeTimeField = i;
                    _activeDeparture = true;
                    _previousMouse = mouse;
                    _previousKeyboard = keyboard;
                    return;
                }
                timeY += 31;
            }

            if (ButtonRect(335, 44, 290, 30).Contains(mouse.Position))
            {
                SaveSchedule();
                _previousMouse = mouse;
                _previousKeyboard = keyboard;
                return;
            }

            var menuRect = new Rectangle((int)_position.X, (int)_position.Y, 660, (int)GetHeight());
            if (!menuRect.Contains(mouse.Position))
            {
                Close();
                return;
            }
        }

        _previousMouse = mouse;
        _previousKeyboard = keyboard;
    }

    private void HandleKeyboard(KeyboardState keyboard)
    {
        if (_activeTimeField < 0 || _activeTimeField >= _arrivals.Count) return;
        string current = _activeDeparture ? _departures[_activeTimeField] : _arrivals[_activeTimeField];
        foreach (Keys key in Enum.GetValues(typeof(Keys)))
        {
            int digit = key switch
            {
                Keys.D0 or Keys.NumPad0 => 0,
                Keys.D1 or Keys.NumPad1 => 1,
                Keys.D2 or Keys.NumPad2 => 2,
                Keys.D3 or Keys.NumPad3 => 3,
                Keys.D4 or Keys.NumPad4 => 4,
                Keys.D5 or Keys.NumPad5 => 5,
                Keys.D6 or Keys.NumPad6 => 6,
                Keys.D7 or Keys.NumPad7 => 7,
                Keys.D8 or Keys.NumPad8 => 8,
                Keys.D9 or Keys.NumPad9 => 9,
                _ => -1
            };
            if (digit < 0 || !IsKeyPressed(keyboard, key)) continue;
            current = current == "00:00" ? digit.ToString(CultureInfo.InvariantCulture) : current + digit.ToString(CultureInfo.InvariantCulture);
        }

        if (IsKeyPressed(keyboard, Keys.Back))
            current = current.Length > 0 ? current[..^1] : "";
        if (IsKeyPressed(keyboard, Keys.Enter))
            current = NormalizeTimeText(current);

        if (_activeDeparture) _departures[_activeTimeField] = current;
        else _arrivals[_activeTimeField] = current;
    }

    private void SaveSchedule()
    {
        if (_wagon == null || _stations == null) return;
        var baseRoute = GetBaseRoute();
        if (baseRoute.Count < 2)
        {
            _status = "Rozkład wymaga co najmniej dwóch stacji.";
            return;
        }

        var schedule = new WagonSchedule
        {
            Id = _wagon.Schedule?.Id ?? Guid.NewGuid(),
            Name = _wagon.ShortName + " — rozkład",
            BaseStationIds = baseRoute.ToList(),
            Enabled = true
        };
        schedule.BuildLoopFromBaseRoute();
        EnsureTimeFields(schedule.Points.Count);

        for (int i = 0; i < schedule.Points.Count; i++)
        {
            if (!TryParseTime(_arrivals[i], out int arrival) || !TryParseTime(_departures[i], out int departure))
            {
                _status = $"Nieprawidłowy czas w punkcie {i + 1}. Użyj HH:MM.";
                return;
            }
            schedule.Points[i].ArrivalSeconds = arrival;
            schedule.Points[i].DepartureSeconds = departure;
        }

        if (!schedule.IsValid(out string error))
        {
            _status = error;
            return;
        }

        _wagon.Route.Clear();
        foreach (Guid stationId in baseRoute)
            _wagon.Route.AddStation(stationId);
        _wagon.SetSchedule(schedule);
        _status = "Rozkład zapisany.";
        Changed();
        Close();
    }

    private List<Guid> GetBaseRoute()
    {
        if (_wagon?.Schedule?.BaseStationIds.Count >= 2)
            return _wagon.Schedule.BaseStationIds.ToList();
        return _wagon?.Route.StationIds.ToList() ?? new List<Guid>();
    }

    private void ApplyBaseRoute(List<Guid> baseRoute)
    {
        var oldArrivals = _arrivals.ToList();
        var oldDepartures = _departures.ToList();
        var loop = baseRoute.Concat(baseRoute.Skip(1).Reverse()).ToList();
        _arrivals.Clear();
        _departures.Clear();
        for (int i = 0; i < loop.Count; i++)
        {
            _arrivals.Add(i < oldArrivals.Count ? oldArrivals[i] : "00:00");
            _departures.Add(i < oldDepartures.Count ? oldDepartures[i] : "00:00");
        }
        _status = "Trasa zmieniona — uzupełnij czasy.";
    }

    private void EnsureTimeFields(int count)
    {
        while (_arrivals.Count < count) _arrivals.Add("00:00");
        while (_departures.Count < count) _departures.Add("00:00");
        while (_arrivals.Count > count) _arrivals.RemoveAt(_arrivals.Count - 1);
        while (_departures.Count > count) _departures.RemoveAt(_departures.Count - 1);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen || _wagon == null || _stations == null || _font == null || _pixel == null) return;
        ClampToViewport();
        float width = 660f, height = GetHeight();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        spriteBatch.Draw(_pixel, new Rectangle((int)_position.X, (int)_position.Y, (int)width, (int)height), new Color(18, 18, 18, 248));
        spriteBatch.Draw(_pixel, new Rectangle((int)_position.X, (int)_position.Y, (int)width, 3), Color.Yellow);
        DrawString(spriteBatch, "ROZKŁAD WAGONU  [S]", 8, 8, Color.Yellow, 0.75f);
        DrawString(spriteBatch, $"Wagon: {_wagon.ShortName}", 8, 27, Color.White, 0.6f);
        DrawButton(spriteBatch, ButtonRect(0, 44, 650, 30), "ZAPISZ ROZKŁAD", false);

        var baseRoute = GetBaseRoute();
        int routeY = 84;
        DrawString(spriteBatch, "TRASA BAZOWA — kliknij stację, aby usunąć", 8, routeY - 18, Color.LightGray, 0.55f);
        for (int i = 0; i < baseRoute.Count; i++)
        {
            var station = _stations.Stations.FirstOrDefault(s => s.Id == baseRoute[i]);
            DrawButton(spriteBatch, ButtonRect(0, routeY, 320, 28), $"{(char)('A' + i)}. {station?.Name ?? "BRAK"}  [USUŃ]", false);
            routeY += 31;
        }

        int addY = routeY + 4;
        foreach (var station in _stations.Stations)
        {
            if (baseRoute.Contains(station.Id)) continue;
            DrawButton(spriteBatch, ButtonRect(0, addY, 320, 28), $"+ {station.Name}", false);
            addY += 31;
        }

        var loop = baseRoute.Concat(baseRoute.Skip(1).Reverse()).ToList();
        int timeY = 84;
        DrawString(spriteBatch, "PUNKTY KONTROLNE", 335, timeY - 18, Color.LightGray, 0.55f);
        DrawString(spriteBatch, "PRZYJAZD", 335, timeY - 2, Color.LightGray, 0.45f);
        DrawString(spriteBatch, "ODJAZD", 485, timeY - 2, Color.LightGray, 0.45f);
        timeY += 16;
        for (int i = 0; i < loop.Count; i++)
        {
            var station = _stations.Stations.FirstOrDefault(s => s.Id == loop[i]);
            bool terminal = i == 0 || i == loop.Count - 1;
            DrawString(spriteBatch, $"{i + 1}. {station?.Name ?? "BRAK"}{(terminal ? " *" : "")}", 335, timeY + 7, Color.White, 0.52f);
            DrawTimeField(spriteBatch, TimeRect(335, timeY, 140, 28), _arrivals.ElementAtOrDefault(i) ?? "00:00", _activeTimeField == i && !_activeDeparture);
            DrawTimeField(spriteBatch, TimeRect(485, timeY, 140, 28), _departures.ElementAtOrDefault(i) ?? "00:00", _activeTimeField == i && _activeDeparture);
            timeY += 31;
        }

        DrawString(spriteBatch, "* terminal: dłuższy postój/manewry ustawiasz różnicą przyjazd → odjazd", 335, timeY + 4, Color.LightGray, 0.43f);
        DrawString(spriteBatch, "Kliknij pole czasu i wpisuj cyfry. Enter normalizuje zapis.", 335, timeY + 22, Color.LightGray, 0.43f);
        if (!string.IsNullOrEmpty(_status))
            DrawString(spriteBatch, _status, 8, Math.Max(timeY + 45, addY + 8), Color.Yellow, 0.48f);
        spriteBatch.End();
    }

    private float GetHeight()
    {
        var baseRoute = GetBaseRoute();
        int loopCount = Math.Max(2, baseRoute.Count * 2 - 1);
        int rows = Math.Max(loopCount, baseRoute.Count + (_stations?.Stations.Count ?? 0) + 2);
        return Math.Min(850f, 105f + rows * 31f);
    }

    private Rectangle ButtonRect(int x, int y, int width, int height) =>
        new((int)_position.X + 5 + x, (int)_position.Y + y, width, height);

    private Rectangle TimeRect(int x, int y, int width, int height) =>
        new((int)_position.X + x, (int)_position.Y + y, width, height);

    private void DrawTimeField(SpriteBatch batch, Rectangle rect, string text, bool active)
    {
        batch.Draw(_pixel!, rect, active ? new Color(80, 80, 35, 250) : new Color(45, 45, 45, 250));
        DrawString(batch, text, rect.X + 8, rect.Y + 7, Color.White, 0.55f);
    }

    private void DrawButton(SpriteBatch batch, Rectangle rect, string text, bool selected)
    {
        batch.Draw(_pixel!, rect, selected ? new Color(70, 90, 35, 245) : new Color(45, 45, 45, 245));
        DrawString(batch, text, rect.X + 6, rect.Y + 6, Color.White, 0.5f);
    }

    private void DrawString(SpriteBatch batch, string text, float x, float y, Color color, float scale) =>
        batch.DrawString(_font!, text, _position + new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    private bool IsKeyPressed(KeyboardState keyboard, Keys key) =>
        keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    private static string FormatTime(int seconds)
    {
        if (seconds < 0) seconds = 0;
        return TimeSpan.FromSeconds(seconds).ToString("hh\\:mm", CultureInfo.InvariantCulture);
    }

    private static string NormalizeTimeText(string text)
    {
        if (TryParseTime(text, out int seconds)) return FormatTime(seconds);
        return text;
    }

    private static bool TryParseTime(string text, out int seconds)
    {
        seconds = 0;
        if (!TimeSpan.TryParseExact(text.Trim(), new[] { "hh\\:mm", "h\\:mm", "hh\\:mm\\:ss", "h\\:mm\\:ss" }, CultureInfo.InvariantCulture, out TimeSpan value))
            return false;
        if (value < TimeSpan.Zero || value.TotalSeconds >= 24 * 60 * 60) return false;
        seconds = (int)value.TotalSeconds;
        return true;
    }

    private void ClampToViewport()
    {
        float width = 660f, height = GetHeight();
        _position.X = MathHelper.Clamp(_position.X, 4f, Math.Max(4f, _graphicsDevice.Viewport.Width - width - 4f));
        _position.Y = MathHelper.Clamp(_position.Y, 4f, Math.Max(4f, _graphicsDevice.Viewport.Height - Math.Min(height, _graphicsDevice.Viewport.Height - 8f) - 4f));
    }
}
