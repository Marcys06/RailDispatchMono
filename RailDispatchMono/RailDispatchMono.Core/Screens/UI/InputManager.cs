using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Train;
using System;
using System.Diagnostics;

namespace RailDispatchMono.Core.Screens.UI
{
    public class InputManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly Camera _camera;
        private readonly TrackBuilder _builder;
        private readonly TrackRenderer _renderer;
        private readonly TrainManager _trainManager;
        private readonly TrainRenderer _trainRenderer;
        private readonly JunctionRadialMenu _junctionRadialMenu;
        private readonly GameMap _map;

        // ============================================================
        // NOWE POLA DLA SEMAFORÓW
        // ============================================================
        private readonly SignalController _signalController;
        private readonly SignalRadialMenu _signalRadialMenu;
        private readonly SignalDirectionMenu _signalDirectionMenu;
        private readonly SignalSelectionMenu _signalSelectionMenu;

        // SignalRenderer do rysowania semaforów
        private SignalRenderer _signalRenderer;

        private MouseState _previousMouse;
        private KeyboardState _previousKeyboard;
        private int _previousScrollWheelValue;

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
            _signalSelectionMenu = signalSelectionMenu;
            _map = map;

            _previousMouse = Mouse.GetState();
            _previousKeyboard = Keyboard.GetState();
            _previousScrollWheelValue = _previousMouse.ScrollWheelValue;

            // ============================================================
            // TWORZENIE SIGNALRENDERER
            // ============================================================
            _signalRenderer = new SignalRenderer(_map, _signalController);
            _signalRenderer.LoadContent(_graphicsDevice);

            // ============================================================
            // SUBKRYPCJA ZDARZEŃ MENU KIERUNKÓW
            // ============================================================
            _signalDirectionMenu.DirectionSelected += OnDirectionSelected;
            _signalDirectionMenu.MenuClosed += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("[SIGNAL] Menu kierunków zamknięte");// $2
            };

            System.Diagnostics.Debug.WriteLine("[INPUT] InputManager utworzony z SignalController i SignalRenderer");// $2
        }

        // ============================================================
        // OBSŁUGA WYBORU KIERUNKU Z MENU
        // ============================================================
        private void OnDirectionSelected(object? sender, SignalDirectionMenu.SignalDirectionSelectedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[INPUT] OnDirectionSelected - kierunek: {e.Direction} dla {e.Position}");// $2

            bool result = _signalController.AddSignal(e.Position, e.Direction);
            System.Diagnostics.Debug.WriteLine($"[INPUT] AddSignal({e.Position}, {e.Direction}) = {result}");// $2

            if (result)
            {
                System.Diagnostics.Debug.WriteLine("[INPUT] ✅ Semafor dodany pomyślnie!");// $2

                // Sprawdź ile jest teraz semaforów
                int total = 0;
                foreach (var kvp in _signalController.Signals)
                    total += kvp.Value.Count;
                System.Diagnostics.Debug.WriteLine($"[INPUT] Łączna liczba semaforów: {total}");// $2
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[INPUT] ❌ Nie udało się dodać semafora!");// $2
            }
        }

        public void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // ============================================================
            // MENU RADIALNE - PRIORYTET
            // ============================================================

            if (_junctionRadialMenu.IsOpen)
            {
                _junctionRadialMenu.Update(mouse, _previousMouse);

                if (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                {
                    _junctionRadialMenu.Close();
                }

                _previousMouse = mouse;
                _previousKeyboard = keyboard;
                _previousScrollWheelValue = mouse.ScrollWheelValue;
                return;
            }

            if (_signalRadialMenu.IsOpen)
            {
                _signalRadialMenu.Update(mouse, _previousMouse);

                if (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                {
                    _signalRadialMenu.Close();
                }

                _previousMouse = mouse;
                _previousKeyboard = keyboard;
                _previousScrollWheelValue = mouse.ScrollWheelValue;
                return;
            }

            if (_signalDirectionMenu.IsOpen)
            {
                _signalDirectionMenu.Update(mouse, _previousMouse);

                if (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                {
                    _signalDirectionMenu.Close();
                }

                _previousMouse = mouse;
                _previousKeyboard = keyboard;
                _previousScrollWheelValue = mouse.ScrollWheelValue;
                return;
            }

            if (_signalSelectionMenu.IsOpen)
            {
                _signalSelectionMenu.Update(mouse, _previousMouse);

                if (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                {
                    _signalSelectionMenu.Close();
                }

                _previousMouse = mouse;
                _previousKeyboard = keyboard;
                _previousScrollWheelValue = mouse.ScrollWheelValue;
                return;
            }

            // ============================================================
            // OBSŁUGA KAMERY (przed innymi akcjami)
            // ============================================================

            // 1. Przesuwanie kamery - środkowy przycisk myszy
            if (mouse.MiddleButton == ButtonState.Pressed && _previousMouse.MiddleButton == ButtonState.Pressed)
            {
                var delta = new Vector2(
                    mouse.X - _previousMouse.X,
                    mouse.Y - _previousMouse.Y);

                if (_camera.Zoom > 0f)
                {
                    _camera.Move(-delta / _camera.Zoom);
                }
            }

            // 2. Zoom - kółko myszy
            var currentScroll = mouse.ScrollWheelValue;
            if (currentScroll != _previousScrollWheelValue)
            {
                var delta = currentScroll > _previousScrollWheelValue ? 2f : -2f;
                var mouseScreenPosition = new Vector2(mouse.X, mouse.Y);
                _camera.ZoomAt(mouseScreenPosition, delta);
            }
            _previousScrollWheelValue = currentScroll;

            // ============================================================
            // OBSŁUGA KLAWIATURY
            // ============================================================
            HandleKeyboardInput(keyboard);

            // ============================================================
            // OBSŁUGA MYSZY (budowanie i semafory)
            // ============================================================
            HandleMouseInput(mouse);

            _previousMouse = mouse;
            _previousKeyboard = keyboard;
        }

        private void HandleKeyboardInput(KeyboardState keyboard)
        {
            // Tryby budowania (1-4)
            if (IsKeyPressed(keyboard, Keys.D1))
            {
                _builder.Mode = TrackBuildMode.Straight;
                Debug.WriteLine("[INPUT] Tryb: Straight");
            }
            if (IsKeyPressed(keyboard, Keys.D2))
            {
                _builder.Mode = TrackBuildMode.Curve;
                Debug.WriteLine("[INPUT] Tryb: Curve");
            }
            if (IsKeyPressed(keyboard, Keys.D3))
            {
                _builder.Mode = TrackBuildMode.Junction;
                Debug.WriteLine("[INPUT] Tryb: Junction");
            }
            if (IsKeyPressed(keyboard, Keys.D4) || IsKeyPressed(keyboard, Keys.NumPad4))
            {
                _builder.Mode = TrackBuildMode.Signal;
                Debug.WriteLine("[INPUT] Tryb: Signal");
            }

            // ============================================================
            // KLAWISZ R - OBRÓT / ZMIANA
            // ============================================================
            if (IsKeyPressed(keyboard, Keys.R))
            {
                if (_builder.Mode == TrackBuildMode.Straight)
                {
                    _builder.StraightHorizontal = !_builder.StraightHorizontal;
                    Debug.WriteLine($"[INPUT] R - StraightHorizontal: {_builder.StraightHorizontal}");
                }
                else if (_builder.Mode == TrackBuildMode.Curve)
                {
                    _builder.Curve = (CurveDirection)(((int)_builder.Curve + 1) % 4);
                    Debug.WriteLine($"[INPUT] R - Curve: {_builder.Curve}");
                }
                else if (_builder.Mode == TrackBuildMode.Junction)
                {
                    _builder.Junction = (JunctionType)(((int)_builder.Junction + 1) % 8);
                    Debug.WriteLine($"[INPUT] R - Junction: {_builder.Junction}");
                }
            }

            // ============================================================
            // KLAWISZ J - PRZEŁĄCZANIE
            // ============================================================
            if (IsKeyPressed(keyboard, Keys.J))
            {
                var mouse = Mouse.GetState();
                var mouseScreenPos = new Vector2(mouse.X, mouse.Y);
                var mapPos = _camera.ScreenToMap(mouseScreenPos);
                var worldPos = new MapPosition((int)mapPos.X, (int)mapPos.Y);

                // Sprawdź semafor
                var signals = _signalController.GetSignalsAt(worldPos);
                if (signals.Count > 0)
                {
                    foreach (var signal in signals)
                    {
                        if (signal.Aspect == SignalAspect.Stop)
                            signal.SetAspect(SignalAspect.Clear);
                        else if (signal.Aspect == SignalAspect.Clear)
                            signal.SetAspect(SignalAspect.Stop);
                        else
                            signal.SetAspect(SignalAspect.Stop);
                    }
                    Debug.WriteLine($"[INPUT] J - przełączono semafor na {worldPos}");
                    return;
                }

                // Sprawdź zwrotnicę
                if (_map.TryGetTrack(worldPos, out var track) && track is not null && track.IsJunction)
                {
                    track.ToggleSwitch();
                    Debug.WriteLine($"[INPUT] J - przełączono zwrotnicę na {worldPos}");
                }
                else
                {
                    Debug.WriteLine($"[INPUT] J - brak semafora lub zwrotnicy na {worldPos}");
                }
            }
        }

        private bool IsKeyPressed(KeyboardState keyboard, Keys key)
        {
            return keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
        }

        private void HandleMouseInput(MouseState mouse)
        {
            var mouseScreenPosition = new Vector2(mouse.X, mouse.Y);
            var worldPosition = _camera.ScreenToMap(mouseScreenPosition);
            var mapPos = new MapPosition((int)worldPosition.X, (int)worldPosition.Y);

            // ============================================================
            // LEWY PRZYCISK - BUDOWANIE
            // ============================================================
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                // Tryb semaforów
                if (_builder.Mode == TrackBuildMode.Signal)
                {
                    if (_map.TryGetTrack(mapPos, out var track) && track != null)
                    {
                        var directions = track.GetAvailableDirections();
                        if (directions.Count == 0)
                            return;

                        if (directions.Count == 1)
                        {
                            _signalController.AddSignal(mapPos, directions[0]);
                            Debug.WriteLine($"[INPUT] SIGNAL - dodano semafor na {mapPos} w kierunku {directions[0]}");
                        }
                        else
                        {
                            _signalDirectionMenu.Open(mouseScreenPosition, mapPos, directions);
                            Debug.WriteLine($"[INPUT] SIGNAL - otwarto menu kierunków na {mapPos}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"[INPUT] SIGNAL - brak toru na {mapPos}");
                    }
                }
                else
                {
                    _builder.BuildAt(mapPos);
                    Debug.WriteLine($"[INPUT] Budowanie na {mapPos}, tryb: {_builder.Mode}");
                }
            }

            // ============================================================
            // PRAWY PRZYCISK - USUWANIE LUB MENU
            // ============================================================
            if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
            {
                Debug.WriteLine($"[INPUT] PPM na {mapPos}");

                // 1. Sprawdź czy na tej pozycji jest semafor
                var signals = _signalController.GetSignalsAt(mapPos);
                if (signals.Count == 1)
                {
                    _signalRadialMenu.Open(mouseScreenPosition, signals[0]);
                    return;
                }
                else if (signals.Count > 1)
                {
                    _signalSelectionMenu.Open(mouseScreenPosition, signals);
                    return;
                }

                // 2. Sprawdź czy jest rozjazd
                if (_map.TryGetTrack(mapPos, out var track) && track != null && track.IsJunction)
                {
                    _junctionRadialMenu.Open(mouseScreenPosition, track);
                    return;
                }

                // 3. USUWANIE TORU / ZAKRĘTU / ROZJAZDU
                if (_map.TryGetTrack(mapPos, out var existingTrack) && existingTrack != null)
                {
                    Debug.WriteLine($"[INPUT] PPM - usuwam tor na {mapPos}");
                    _builder.Remove(mapPos);
                    return;
                }

                Debug.WriteLine($"[INPUT] PPM - brak elementu do usunięcia na {mapPos}");
            }
        }

        public void Draw(GameTime gameTime)
        {
            _graphicsDevice.Clear(Color.CornflowerBlue);

            var mouse = Mouse.GetState();
            var mouseScreenPosition = new Vector2(mouse.X, mouse.Y);
            var previewPosition = _camera.ScreenToMap(mouseScreenPosition);

            _spriteBatch.Begin(
                transformMatrix: _camera.Transform,
                samplerState: SamplerState.PointClamp
            );

            _renderer.Draw(_spriteBatch, _camera);

            // ============================================================
            // RYSUJ SEMAFORY PRZY UŻYCIU SIGNALRENDERER
            // ============================================================
            _signalRenderer.Draw(_spriteBatch, _camera);

            _trainRenderer.Draw(_spriteBatch, _trainManager);

            _renderer.DrawPreview(
                _spriteBatch,
                previewPosition,
                _builder.Mode,
                _builder.StraightHorizontal,
                _builder.Curve,
                _builder.Junction);

            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (_junctionRadialMenu.IsOpen)
            {
                _junctionRadialMenu.Draw(_spriteBatch);
            }

            if (_signalRadialMenu.IsOpen)
            {
                _signalRadialMenu.Draw(_spriteBatch);
            }

            if (_signalDirectionMenu.IsOpen)
            {
                _signalDirectionMenu.Draw(_spriteBatch);
            }

            if (_signalSelectionMenu.IsOpen)
            {
                _signalSelectionMenu.Draw(_spriteBatch);
            }

            _spriteBatch.End();
        }
    }
}