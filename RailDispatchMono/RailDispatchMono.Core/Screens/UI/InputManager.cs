using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.ScreenManagers;
using System;
using System.Linq;

namespace RailDispatchMono.Core.Screens.UI
{
    public class InputManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Camera _camera;
        private readonly TrackBuilder _builder;
        private readonly ScreenManager _screenManager;
        private readonly TrackRenderer _renderer;
        private readonly TrainManager _trainManager;
        private readonly TrainRenderer _trainRenderer;
        private readonly JunctionRadialMenu _junctionRadialMenu;
        private readonly GameMap _map;
        private readonly SignalController _signalController;
        private readonly SignalRadialMenu _signalRadialMenu;
        private readonly SignalDirectionMenu _signalDirectionMenu;
        private readonly SignalSelectionMenu _signalSelectionMenu;
        private readonly StationController _stationController;
        private readonly StationRenderer _stationRenderer;
        private readonly SignalRenderer _signalRenderer;
        private SpriteFont? _tooltipFont;
        private Texture2D? _tooltipPixel;
        private MouseState _previousMouse;
        private KeyboardState _previousKeyboard;
        private int _previousScrollWheelValue;

        // Rozmiar stacji wybierany przez użytkownika w trybie Station.
        private int _stationWidth = 1;
        private int _stationHeight = 1;
        private static readonly (int Width, int Height)[] StationSizes =
        {
            (1, 1), (2, 2), (3, 3), (4, 4)
        };
        private int _stationSizeIndex;

        public InputManager(
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            Camera camera,
            TrackBuilder builder,
            TrackRenderer renderer,
            TrainManager trainManager,
            TrainRenderer trainRenderer,
            JunctionRadialMenu junctionRadialMenu,
            SignalController signalController,
            SignalRadialMenu signalRadialMenu,
            ScreenManager screenManager,
            SignalDirectionMenu signalDirectionMenu,
            SignalSelectionMenu signalSelectionMenu,
            GameMap map)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _camera = camera;
            _builder = builder;
            _renderer = renderer;
            _trainManager = trainManager;
            _trainRenderer = trainRenderer;
            _junctionRadialMenu = junctionRadialMenu;
            _signalController = signalController;
            _signalRadialMenu = signalRadialMenu;
            _signalDirectionMenu = signalDirectionMenu;
            _screenManager = screenManager;
            _signalSelectionMenu = signalSelectionMenu;
            _map = map;
            _stationController = _trainManager.StationController;
            _stationRenderer = new StationRenderer(_stationController);
            _stationRenderer.LoadContent(_graphicsDevice);
            _previousMouse = Mouse.GetState();
            _previousKeyboard = Keyboard.GetState();
            _previousScrollWheelValue = _previousMouse.ScrollWheelValue;
            _signalRenderer = new SignalRenderer(_map, _signalController);
            _signalRenderer.LoadContent(_graphicsDevice);
            _signalDirectionMenu.DirectionSelected += OnDirectionSelected;
            _signalDirectionMenu.MenuClosed += (s, e) => DebugManager.Log("[SIGNAL] Menu kierunków zamknięte");
            DebugManager.Log("[INPUT] InputManager utworzony z SignalController, StationController i Rendererami");
        }

        private void OnDirectionSelected(object? sender, SignalDirectionMenu.SignalDirectionSelectedEventArgs e)
        {
            bool result = _signalController.AddSignal(e.Position, e.Direction);
            DebugManager.Log($"[INPUT] SIGNAL AddSignal({e.Position}, {e.Direction}) = {result}");
        }

        public void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();
            if (_junctionRadialMenu.IsOpen)
            {
                _junctionRadialMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape)) _junctionRadialMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }
            if (_signalRadialMenu.IsOpen)
            {
                _signalRadialMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape)) _signalRadialMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }
            if (_signalDirectionMenu.IsOpen)
            {
                _signalDirectionMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape)) _signalDirectionMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }
            if (_signalSelectionMenu.IsOpen)
            {
                _signalSelectionMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape)) _signalSelectionMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }
            if (mouse.MiddleButton == ButtonState.Pressed && _previousMouse.MiddleButton == ButtonState.Pressed)
            {
                Vector2 delta = new(mouse.X - _previousMouse.X, mouse.Y - _previousMouse.Y);
                if (_camera.Zoom > 0f) _camera.Move(-delta / _camera.Zoom);
            }
            int currentScroll = mouse.ScrollWheelValue;
            if (currentScroll != _previousScrollWheelValue)
            {
                float zoomDelta = currentScroll > _previousScrollWheelValue ? 2f : -2f;
                _camera.ZoomAt(new Vector2(mouse.X, mouse.Y), zoomDelta);
            }
            _previousScrollWheelValue = currentScroll;
            HandleKeyboardInput(keyboard);
            HandleMouseInput(mouse, keyboard);
            RememberInput(mouse, keyboard);
        }

        private void HandleKeyboardInput(KeyboardState keyboard)
        {
            if (IsKeyPressed(keyboard, Keys.D1) || IsKeyPressed(keyboard, Keys.NumPad1)) _builder.Mode = TrackBuildMode.Straight;
            if (IsKeyPressed(keyboard, Keys.D2) || IsKeyPressed(keyboard, Keys.NumPad2)) _builder.Mode = TrackBuildMode.Curve;
            if (IsKeyPressed(keyboard, Keys.D3) || IsKeyPressed(keyboard, Keys.NumPad3)) _builder.Mode = TrackBuildMode.Junction;
            if (IsKeyPressed(keyboard, Keys.D4) || IsKeyPressed(keyboard, Keys.NumPad4)) _builder.Mode = TrackBuildMode.Signal;
            if (IsKeyPressed(keyboard, Keys.D5) || IsKeyPressed(keyboard, Keys.NumPad5)) _builder.Mode = TrackBuildMode.Station;

            if (IsKeyPressed(keyboard, Keys.F1)) DebugManager.ToggleCategory(DebugManager.DebugCategory.Block);
            if (IsKeyPressed(keyboard, Keys.F2)) DebugManager.ToggleCategory(DebugManager.DebugCategory.Signal);
            if (IsKeyPressed(keyboard, Keys.F3)) DebugManager.ToggleCategory(DebugManager.DebugCategory.Train);
            if (IsKeyPressed(keyboard, Keys.F4)) DebugManager.ToggleCategory(DebugManager.DebugCategory.TrainMovement);
            if (IsKeyPressed(keyboard, Keys.F5)) ToggleAllDebugCategories();
            if (IsKeyPressed(keyboard, Keys.F12))
            {
                string fileName = $"debug_log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                DebugManager.SaveLogToFile(fileName);
                DebugManager.Log($"[INPUT] Log saved to: {fileName}");
            }
            if (IsKeyPressed(keyboard, Keys.Escape))
            {
                var pauseScreen = new PauseScreen();
                _screenManager.AddScreen(pauseScreen, null);
            }
            if (IsKeyPressed(keyboard, Keys.R))
            {
                if (_builder.Mode == TrackBuildMode.Straight)
                    _builder.StraightHorizontal = !_builder.StraightHorizontal;
                else if (_builder.Mode == TrackBuildMode.Curve)
                    _builder.Curve = (CurveDirection)(((int)_builder.Curve + 1) % 4);
                else if (_builder.Mode == TrackBuildMode.Junction)
                    _builder.Junction = (JunctionType)(((int)_builder.Junction + 1) % 8);
                else if (_builder.Mode == TrackBuildMode.Station)
                {
                    _stationSizeIndex = (_stationSizeIndex + 1) % StationSizes.Length;
                    (_stationWidth, _stationHeight) = StationSizes[_stationSizeIndex];
                    DebugManager.Log($"[INPUT] STATION - rozmiar {_stationWidth}x{_stationHeight}");
                }
            }
            if (IsKeyPressed(keyboard, Keys.J)) ToggleSignalOrSwitch();
        }

        private void ToggleAllDebugCategories()
        {
            bool allEnabled = true;
            foreach (DebugManager.DebugCategory category in Enum.GetValues(typeof(DebugManager.DebugCategory)))
            {
                if (category == DebugManager.DebugCategory.All || category == DebugManager.DebugCategory.General) continue;
                if (!DebugManager.IsCategoryEnabled(category)) { allEnabled = false; break; }
            }
            if (allEnabled) DebugManager.DisableAll(); else DebugManager.EnableAll();
        }

        private void ToggleSignalOrSwitch()
        {
            var mouse = Mouse.GetState();
            MapPosition position = ToMapPosition(new Vector2(mouse.X, mouse.Y));
            var signals = _signalController.GetSignalsAt(position);
            if (signals.Count > 0)
            {
                foreach (var signal in signals)
                {
                    if (signal.Aspect == SignalAspect.Stop) signal.SetAspect(SignalAspect.Clear);
                    else if (signal.Aspect == SignalAspect.Clear) signal.SetAspect(SignalAspect.Stop);
                    else signal.SetAspect(SignalAspect.Stop);
                }
                DebugManager.Log($"[INPUT] J - przełączono semafor na {position}");
                return;
            }
            if (_map.TryGetTrack(position, out var track) && track != null && track.IsJunction)
            {
                track.ToggleSwitch();
                DebugManager.Log($"[INPUT] J - przełączono zwrotnicę na {position}");
            }
        }

        private void HandleMouseInput(MouseState mouse, KeyboardState keyboard)
        {
            Vector2 screenPosition = new(mouse.X, mouse.Y);
            MapPosition mapPosition = ToMapPosition(screenPosition);
            bool shiftPressed = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                if (_builder.Mode == TrackBuildMode.Signal) PlaceSignal(mapPosition, screenPosition);
                else if (_builder.Mode == TrackBuildMode.Station) PlaceStation(mapPosition);
                else
                {
                    _builder.BuildAt(mapPosition);
                    DebugManager.Log($"[INPUT] Budowanie na {mapPosition}, tryb: {_builder.Mode}");
                }
            }
            if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
            {
                var signals = _signalController.GetSignalsAt(mapPosition);
                if (signals.Count > 0)
                {
                    if (shiftPressed) _signalController.RemoveSignalsAt(mapPosition);
                    else if (signals.Count == 1) _signalRadialMenu.Open(screenPosition, signals[0]);
                    else _signalSelectionMenu.Open(screenPosition, signals);
                    return;
                }
                var station = _stationController.GetStationAt(mapPosition);
                if (station != null)
                {
                    if (shiftPressed)
                    {
                        _stationController.RemoveStation(station);
                        DebugManager.Log($"[INPUT] STATION - usunięto {station.Name} na {mapPosition}");
                    }
                    return;
                }
                if (_map.TryGetTrack(mapPosition, out var track) && track != null && track.IsJunction)
                {
                    if (shiftPressed) _builder.Remove(mapPosition); else _junctionRadialMenu.Open(screenPosition, track);
                    return;
                }
                if (_map.TryGetTrack(mapPosition, out var existingTrack) && existingTrack != null) _builder.Remove(mapPosition);
            }
        }

        private void PlaceSignal(MapPosition mapPosition, Vector2 screenPosition)
        {
            if (!_map.TryGetTrack(mapPosition, out var track) || track == null)
            {
                DebugManager.Log($"[INPUT] SIGNAL - brak toru na {mapPosition}");
                return;
            }
            var directions = track.GetAvailableDirections();
            if (directions.Count == 0) return;
            if (directions.Count == 1) _signalController.AddSignal(mapPosition, directions[0]);
            else _signalDirectionMenu.Open(screenPosition, mapPosition, directions);
        }

        private void PlaceStation(MapPosition mapPosition)
        {
            int width = _stationWidth;
            int height = _stationHeight;
            var origin = new MapPosition(mapPosition.X - (width - 1) / 2, mapPosition.Y - (height - 1) / 2);

            // Każde pole obszaru musi posiadać tor. Dzięki temu stacja 3x3 faktycznie
            // reprezentuje obszar sieci, a nie pusty prostokąt nad mapą.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new MapPosition(origin.X + x, origin.Y + y);
                    if (!_map.TryGetTrack(cell, out var track) || track == null)
                    {
                        DebugManager.Log($"[INPUT] STATION - brak toru na {cell}; nie utworzono stacji {width}x{height}");
                        return;
                    }
                    if (_stationController.GetStationAt(cell) != null)
                    {
                        DebugManager.Log($"[INPUT] STATION - obszar nachodzi na istniejącą stację na {cell}");
                        return;
                    }
                }
            }

            int number = _stationController.Stations.Count + 1;
            var station = new Station($"Stacja {number}", origin, width, height);
            _stationController.AddStation(station);
            DebugManager.Log($"[INPUT] STATION - dodano {station.Name} {width}x{height} na {origin}");
        }

        private MapPosition ToMapPosition(Vector2 screenPosition) => _camera.ScreenToMap(screenPosition);
        private bool IsKeyPressed(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
        private void RememberInput(MouseState mouse, KeyboardState keyboard)
        {
            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            _previousScrollWheelValue = mouse.ScrollWheelValue;
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.Clear(Color.CornflowerBlue);
            var mouse = Mouse.GetState();
            Vector2 screenPosition = new(mouse.X, mouse.Y);
            MapPosition previewMapPosition = _camera.ScreenToMap(screenPosition);
            _spriteBatch.Begin(transformMatrix: _camera.Transform, samplerState: SamplerState.PointClamp);
            _renderer.Draw(_spriteBatch, _camera);
            _signalRenderer.Draw(_spriteBatch, _camera);
            _stationRenderer.Draw(_spriteBatch);
            _trainRenderer.Draw(_spriteBatch, _trainManager);
            _renderer.DrawPreview(_spriteBatch, previewMapPosition, _builder.Mode, _builder.StraightHorizontal, _builder.Curve, _builder.Junction);
            if (_builder.Mode == TrackBuildMode.Station)
                _stationRenderer.DrawPreview(_spriteBatch, previewMapPosition, _stationWidth, _stationHeight);
            _spriteBatch.End();
            DrawStationTooltip(screenPosition, previewMapPosition);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (_junctionRadialMenu.IsOpen) _junctionRadialMenu.Draw(_spriteBatch);
            if (_signalRadialMenu.IsOpen) _signalRadialMenu.Draw(_spriteBatch);
            if (_signalDirectionMenu.IsOpen) _signalDirectionMenu.Draw(_spriteBatch);
            if (_signalSelectionMenu.IsOpen) _signalSelectionMenu.Draw(_spriteBatch);
            _spriteBatch.End();
        }

        private void DrawStationTooltip(Vector2 mouseScreenPosition, MapPosition mouseMapPosition)
        {
            var station = _stationController.GetStationAt(mouseMapPosition);
            if (station == null) return;
            var font = GetTooltipFont();
            var pixel = GetTooltipPixel();
            if (font == null || pixel == null) return;
            var waiting = _stationController.Passengers.GetWaitingAt(station).ToList();
            int waitingCount = waiting.Count;
            int destinationCount = waiting.Select(p => p.DestinationStation.Id).Distinct().Count();
            string[] lines =
            {
                "STACJA",
                station.Name,
                "ID: " + station.Id.ToString()[..8],
                "Rozmiar: " + station.Width + "x" + station.Height,
                "Oczekujacy: " + waitingCount,
                "Rozne cele: " + destinationCount,
                "Obsluga: " + (station.PassengerServiceEnabled ? "TAK" : "NIE"),
                "Postoj: " + station.DwellTimeSeconds.ToString("F1") + " s"
            };
            float padding = 8f;
            float lineHeight = font.MeasureString("A").Y + 2f;
            float maxWidth = 0f;
            foreach (var line in lines) maxWidth = MathF.Max(maxWidth, font.MeasureString(line).X);
            float tooltipWidth = maxWidth + padding * 2f;
            float tooltipHeight = lines.Length * lineHeight + padding * 2f;
            Vector2 tooltipPos = mouseScreenPosition + new Vector2(15f, 15f);
            var viewport = _graphicsDevice.Viewport;
            if (tooltipPos.X + tooltipWidth > viewport.Width) tooltipPos.X = mouseScreenPosition.X - tooltipWidth - 15f;
            if (tooltipPos.Y + tooltipHeight > viewport.Height) tooltipPos.Y = mouseScreenPosition.Y - tooltipHeight - 15f;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Rectangle bgRect = new((int)tooltipPos.X, (int)tooltipPos.Y, (int)tooltipWidth, (int)tooltipHeight);
            _spriteBatch.Draw(pixel, bgRect, new Color(30, 90, 150, 230));
            const int border = 2;
            var borderColor = new Color(100, 190, 255, 230);
            _spriteBatch.Draw(pixel, new Rectangle(bgRect.X - border, bgRect.Y - border, bgRect.Width + border * 2, border), borderColor);
            _spriteBatch.Draw(pixel, new Rectangle(bgRect.X - border, bgRect.Y + bgRect.Height, bgRect.Width + border * 2, border), borderColor);
            _spriteBatch.Draw(pixel, new Rectangle(bgRect.X - border, bgRect.Y - border, border, bgRect.Height + border * 2), borderColor);
            _spriteBatch.Draw(pixel, new Rectangle(bgRect.X + bgRect.Width, bgRect.Y - border, border, bgRect.Height + border * 2), borderColor);
            Vector2 textPos = tooltipPos + new Vector2(padding, padding);
            for (int i = 0; i < lines.Length; i++)
            {
                _spriteBatch.DrawString(font, lines[i], textPos, i == 0 ? Color.Yellow : Color.White);
                textPos.Y += lineHeight;
            }
            _spriteBatch.End();
        }

        private SpriteFont? GetTooltipFont()
        {
            if (_tooltipFont != null) return _tooltipFont;
            try { _tooltipFont = _screenManager.Game.Content.Load<SpriteFont>("Arial24"); }
            catch (InvalidOperationException) { return null; }
            return _tooltipFont;
        }

        private Texture2D GetTooltipPixel()
        {
            if (_tooltipPixel != null) return _tooltipPixel;
            _tooltipPixel = new Texture2D(_graphicsDevice, 1, 1);
            _tooltipPixel.SetData(new[] { Color.White });
            return _tooltipPixel;
        }
    }
}