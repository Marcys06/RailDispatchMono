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
            if (IsKeyPressed(keyboard, Keys.D1))
            {
                _builder.Mode = TrackBuildMode.Straight;
                System.Diagnostics.Debug.WriteLine("[INPUT] Tryb: Straight");// $2
            }

            if (IsKeyPressed(keyboard, Keys.D2))
            {
                _builder.Mode = TrackBuildMode.Curve;
                System.Diagnostics.Debug.WriteLine("[INPUT] Tryb: Curve");// $2
            }

            if (IsKeyPressed(keyboard, Keys.D3))
            {
                _builder.Mode = TrackBuildMode.Junction;
                System.Diagnostics.Debug.WriteLine("[INPUT] Tryb: Junction");// $2
            }

            if (IsKeyPressed(keyboard, Keys.D4) || IsKeyPressed(keyboard, Keys.NumPad4))
            {
                _builder.Mode = TrackBuildMode.Signal;
                System.Diagnostics.Debug.WriteLine("[INPUT] Tryb: Signal");// $2
            }

            if (keyboard.IsKeyDown(Keys.R) && _previousKeyboard.IsKeyUp(Keys.R))
            {
                if (_builder.Mode == TrackBuildMode.Straight)
                    _builder.StraightHorizontal = !_builder.StraightHorizontal;
            }

            // ============================================================
            // KLAWISZ J - PRZEŁĄCZANIE SEMAFORA / ZWROTNICY
            // ============================================================
            if (IsKeyPressed(keyboard, Keys.J))
            {
                var mouse = Mouse.GetState();
                var mouseScreenPos = new Vector2(mouse.X, mouse.Y);
                var mapPos = _camera.ScreenToMap(mouseScreenPos);
                var worldPos = new MapPosition((int)mapPos.X, (int)mapPos.Y);

                System.Diagnostics.Debug.WriteLine($"[INPUT] J - sprawdzam pozycję {worldPos}");// $2

                // Sprawdź czy jest semafor
                var signals = _signalController.GetSignalsAt(worldPos);
                if (signals.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] J - znaleziono {signals.Count} semaforów");// $2
                    // Przełącz aspekt semafora (Stop <-> Clear)
                    foreach (var signal in signals)
                    {
                        if (signal.Aspect == SignalAspect.Stop)
                            signal.SetAspect(SignalAspect.Clear);
                        else if (signal.Aspect == SignalAspect.Clear)
                            signal.SetAspect(SignalAspect.Stop);
                        else
                            signal.SetAspect(SignalAspect.Stop);
                    }
                    System.Diagnostics.Debug.WriteLine($"[INPUT] J - przełączono semafor na {worldPos}");// $2
                    return;
                }

                // Sprawdź czy jest zwrotnica
                if (_map.TryGetTrack(worldPos, out var track) && track is not null && track.IsJunction)
                {
                    track.ToggleSwitch();
                    System.Diagnostics.Debug.WriteLine($"[INPUT] J - przełączono zwrotnicę na {worldPos}");// $2
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] J - brak semafora lub zwrotnicy na {worldPos}");// $2
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
            // LEWY PRZYCISK - BUDOWANIE / STAWIANIE SEMAFORÓW
            // ============================================================

            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT] LPM na {mapPos}, tryb: {_builder.Mode}");// $2

                // Tryb semaforów
                if (_builder.Mode == TrackBuildMode.Signal)
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - _signalController null? {_signalController == null}");// $2

                    // Sprawdź czy jest tor na tej pozycji
                    if (_map.TryGetTrack(mapPos, out var track) && track != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - znaleziono tor na {mapPos}");// $2

                        // Pobierz dostępne kierunki połączeń toru
                        var directions = track.GetAvailableDirections();
                        System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - kierunki: {string.Join(", ", directions)}");// $2

                        if (directions.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine("[INPUT] SIGNAL - brak kierunków!");// $2
                            return;
                        }

                        // Jeśli tylko jeden kierunek - użyj go
                        if (directions.Count == 1)
                        {
                            System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - dodaję semafor w kierunku {directions[0]}");// $2
                            bool result = _signalController.AddSignal(mapPos, directions[0]);
                            System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - AddSignal({mapPos}, {directions[0]}) = {result}");// $2

                            if (result)
                            {
                                System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - ✅ Semafor dodany pomyślnie!");// $2
                                // Sprawdź ile jest teraz semaforów
                                var allSignals = _signalController.Signals;
                                int total = 0;
                                foreach (var kvp in allSignals)
                                    total += kvp.Value.Count;
                                System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - Łączna liczba semaforów: {total}");// $2
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - ❌ Nie udało się dodać semafora!");// $2
                            }
                        }
                        else
                        {
                            // Jeśli więcej kierunków - pokaż menu wyboru kierunku
                            System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - otwieram menu wyboru kierunku");// $2
                            _signalDirectionMenu.Open(mouseScreenPosition, mapPos, directions);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[INPUT] SIGNAL - ❌ Brak toru na {mapPos}");// $2
                    }
                }
                else
                {
                    // Normalne budowanie (torów, rozjazdów itp.)
                    System.Diagnostics.Debug.WriteLine($"[INPUT] Budowanie na {mapPos}, tryb: {_builder.Mode}");// $2
                    _builder.BuildAt(mapPos);
                }
            }

            // ============================================================
            // PRAWY PRZYCISK - INTERAKCJA Z SEMAFOREM / ROZJAZDEM
            // ============================================================

            if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT] PPM na {mapPos}");

                // Sprawdź czy na tej pozycji jest semafor
                var signals = _signalController.GetSignalsAt(mapPos);
                System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - znaleziono {signals.Count} semaforow");

                // DODAJ TEN LOG:
                System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - _signalRadialMenu is null? {_signalRadialMenu == null}");

                if (signals.Count == 1)
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - otwieram menu aspektow dla semafora");
                    _signalRadialMenu.Open(mouseScreenPosition, signals[0]);
                    System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - po Open, IsOpen: {_signalRadialMenu.IsOpen}");
                }
                else if (signals.Count > 1)
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - otwieram menu wyboru semafora");
                    _signalSelectionMenu.Open(mouseScreenPosition, signals);
                }
                else if (_map.TryGetTrack(mapPos, out var track) && track != null && track.IsJunction)
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - otwieram menu rozjazdu");
                    _junctionRadialMenu.Open(mouseScreenPosition, track);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[INPUT] PPM - brak semafora lub rozjazdu na {mapPos}");
                }
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