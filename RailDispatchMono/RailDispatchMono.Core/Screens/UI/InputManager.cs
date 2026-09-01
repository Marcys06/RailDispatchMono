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
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
        private readonly DepotController _depotController;
        private readonly DepotRenderer _depotRenderer;
        private readonly SignalRenderer _signalRenderer;
        private readonly WagonRouteMenu _wagonRouteMenu;
        private readonly DepotTrainMenu _depotTrainMenu;

        private SpriteFont? _tooltipFont;
        private Texture2D? _tooltipPixel;
        private MouseState _previousMouse;
        private KeyboardState _previousKeyboard;
        private int _previousScrollWheelValue;
        private bool _wagonRouteEditMode;
        private bool _spawnArmed;

        private int _selectedPassengerWagons = 2;
        private int _stationWidth = 1, _stationHeight = 1;
        private static readonly (int Width, int Height)[] StationSizes = { (1, 1), (2, 2), (3, 3), (4, 4) };
        private int _stationSizeIndex;

        public event Action<Depot>? DepotSelected;

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
            GameMap map,
            DepotController? depotController = null)
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

            _depotController = depotController ?? new DepotController();
            _depotRenderer = new DepotRenderer(_depotController);
            _depotRenderer.LoadContent(_graphicsDevice);

            _previousMouse = Mouse.GetState();
            _previousKeyboard = Keyboard.GetState();
            _previousScrollWheelValue = _previousMouse.ScrollWheelValue;

            _signalRenderer = new SignalRenderer(_map, _signalController);
            _signalRenderer.LoadContent(_graphicsDevice);

            _signalDirectionMenu.DirectionSelected += OnDirectionSelected;

            _wagonRouteMenu = new WagonRouteMenu(_graphicsDevice);
            _wagonRouteMenu.LoadContent();
            _wagonRouteMenu.RouteChanged += OnWagonRouteChanged;

            _depotTrainMenu = new DepotTrainMenu(_graphicsDevice);
            _depotTrainMenu.LoadContent();
            _depotTrainMenu.ConsistSelected += OnConsistSelected;
        }

        private void OnDirectionSelected(object? sender, SignalDirectionMenu.SignalDirectionSelectedEventArgs e)
            => _signalController.AddSignal(e.Position, e.Direction);

        private void OnConsistSelected(int passengerWagons)
        {
            _selectedPassengerWagons = Math.Clamp(passengerWagons, 1, 8);
            _spawnArmed = true;
            _builder.Mode = TrackBuildMode.None;
        }

        public void SetFont(SpriteFont font)
        {
            _tooltipFont = font;
            _wagonRouteMenu.SetFont(font);
            _depotTrainMenu.SetFont(font);
        }

        public void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // Obsługa otwartych menu
            if (_depotTrainMenu.IsOpen)
            {
                _depotTrainMenu.Update(mouse);
                if (IsKeyPressed(keyboard, Keys.Escape))
                    _depotTrainMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }

            if (_wagonRouteMenu.IsOpen)
            {
                _wagonRouteMenu.Update(mouse);
                if (IsKeyPressed(keyboard, Keys.Escape))
                    _wagonRouteMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }

            if (_junctionRadialMenu.IsOpen)
            {
                _junctionRadialMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape))
                    _junctionRadialMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }

            if (_signalRadialMenu.IsOpen)
            {
                _signalRadialMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape))
                    _signalRadialMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }

            if (_signalDirectionMenu.IsOpen)
            {
                _signalDirectionMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape))
                    _signalDirectionMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }

            if (_signalSelectionMenu.IsOpen)
            {
                _signalSelectionMenu.Update(mouse, _previousMouse);
                if (IsKeyPressed(keyboard, Keys.Escape))
                    _signalSelectionMenu.Close();
                RememberInput(mouse, keyboard);
                return;
            }

            // Kamera – przesuwanie
            if (mouse.MiddleButton == ButtonState.Pressed && _previousMouse.MiddleButton == ButtonState.Pressed)
            {
                Vector2 delta = new(mouse.X - _previousMouse.X, mouse.Y - _previousMouse.Y);
                if (_camera.Zoom > 0)
                    _camera.Move(-delta / _camera.Zoom);
            }

            // Kamera – zoom
            int scroll = mouse.ScrollWheelValue;
            if (scroll != _previousScrollWheelValue)
                _camera.ZoomAt(new Vector2(mouse.X, mouse.Y), scroll > _previousScrollWheelValue ? 2f : -2f);
            _previousScrollWheelValue = scroll;

            HandleKeyboardInput(keyboard);
            HandleMouseInput(mouse, keyboard);
            RememberInput(mouse, keyboard);
        }

        private void HandleKeyboardInput(KeyboardState keyboard)
        {
            // Tryby budowania
            if (IsKeyPressed(keyboard, Keys.D1) || IsKeyPressed(keyboard, Keys.NumPad1))
                _builder.Mode = TrackBuildMode.Straight;
            if (IsKeyPressed(keyboard, Keys.D2) || IsKeyPressed(keyboard, Keys.NumPad2))
                _builder.Mode = TrackBuildMode.Curve;
            if (IsKeyPressed(keyboard, Keys.D3) || IsKeyPressed(keyboard, Keys.NumPad3))
                _builder.Mode = TrackBuildMode.Junction;
            if (IsKeyPressed(keyboard, Keys.D4) || IsKeyPressed(keyboard, Keys.NumPad4))
                _builder.Mode = TrackBuildMode.Signal;
            if (IsKeyPressed(keyboard, Keys.D5) || IsKeyPressed(keyboard, Keys.NumPad5))
                _builder.Mode = TrackBuildMode.Station;
            if (IsKeyPressed(keyboard, Keys.D9) || IsKeyPressed(keyboard, Keys.NumPad9))
                _builder.Mode = TrackBuildMode.Depot;

            // Tryb edycji tras wagonów
            if (IsKeyPressed(keyboard, Keys.S))
            {
                _wagonRouteEditMode = !_wagonRouteEditMode;
                _builder.Mode = TrackBuildMode.None;
                _spawnArmed = false;
                if (!_wagonRouteEditMode)
                    _wagonRouteMenu.Close();
            }

            // Debug – F1-F5, F12
            if (IsKeyPressed(keyboard, Keys.F1))
                DebugManager.ToggleCategory(DebugManager.DebugCategory.Block);
            if (IsKeyPressed(keyboard, Keys.F2))
                DebugManager.ToggleCategory(DebugManager.DebugCategory.Signal);
            if (IsKeyPressed(keyboard, Keys.F3))
                DebugManager.ToggleCategory(DebugManager.DebugCategory.Train);
            if (IsKeyPressed(keyboard, Keys.F4))
                DebugManager.ToggleCategory(DebugManager.DebugCategory.TrainMovement);
            if (IsKeyPressed(keyboard, Keys.F5))
                ToggleAllDebugCategories();
            if (IsKeyPressed(keyboard, Keys.F12))
            {
                string file = $"debug_log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                DebugManager.SaveLogToFile(file);
            }

            // ESC – zamyka menu i tryby, NIE otwiera pauzy (pauza jest w GameplayScreen)
            if (IsKeyPressed(keyboard, Keys.Escape))
            {
                if (_wagonRouteEditMode)
                {
                    _wagonRouteEditMode = false;
                    _wagonRouteMenu.Close();
                }
                else if (_spawnArmed)
                {
                    _spawnArmed = false;
                }
                // Pauza jest teraz zarządzana przez GameplayScreen, nie przez InputManager
            }

            // R – rotacja/zmiana
            if (IsKeyPressed(keyboard, Keys.R))
            {
                if (_builder.Mode == TrackBuildMode.Straight)
                    _builder.StraightHorizontal = !_builder.StraightHorizontal;
                else if (_builder.Mode == TrackBuildMode.Curve)
                    _builder.Curve = (CurveDirection)(((int)_builder.Curve + 1) % 4);
                else if (_builder.Mode == TrackBuildMode.Junction)
                    _builder.Junction = (JunctionType)(((int)_builder.Junction + 1) % 12);
                else if (_builder.Mode == TrackBuildMode.Station)
                {
                    _stationSizeIndex = (_stationSizeIndex + 1) % StationSizes.Length;
                    (_stationWidth, _stationHeight) = StationSizes[_stationSizeIndex];
                }
            }

            // J – przełącz semafor/rozjazd
            if (IsKeyPressed(keyboard, Keys.J))
                ToggleSignalOrSwitch();
        }

        private void ToggleAllDebugCategories()
        {
            bool all = true;
            foreach (DebugManager.DebugCategory c in Enum.GetValues(typeof(DebugManager.DebugCategory)))
            {
                if (c == DebugManager.DebugCategory.All || c == DebugManager.DebugCategory.General)
                    continue;
                if (!DebugManager.IsCategoryEnabled(c))
                {
                    all = false;
                    break;
                }
            }
            if (all)
                DebugManager.DisableAll();
            else
                DebugManager.EnableAll();
        }

        private void ToggleSignalOrSwitch()
        {
            var mouse = Mouse.GetState();
            var pos = ToMapPosition(new Vector2(mouse.X, mouse.Y));

            var signals = _signalController.GetSignalsAt(pos);
            if (signals.Count > 0)
            {
                foreach (var signal in signals)
                    signal.SetAspect(signal.Aspect == SignalAspect.Stop ? SignalAspect.Clear : SignalAspect.Stop);
                return;
            }

            if (_map.TryGetTrack(pos, out var track) && track != null && track.IsJunction)
                track.ToggleSwitch();
        }

        private void HandleMouseInput(MouseState mouse, KeyboardState keyboard)
        {
            Vector2 screen = new(mouse.X, mouse.Y);
            MapPosition pos = ToMapPosition(screen);
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

            // LEWY PRZYCISK
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                // Tryb edycji tras wagonów
                if (_wagonRouteEditMode)
                {
                    var vehicle = _trainRenderer.GetVehicleAtPosition(_trainManager, _camera.ScreenToWorld(screen));
                    if (vehicle.HasValue && vehicle.Value.train.Composition.Vehicles[vehicle.Value.vehicleIndex] is Wagon wagon)
                    {
                        _wagonRouteMenu.Open(screen, wagon, _stationController);
                        return;
                    }
                    return;
                }

                // Tryb spawnu
                if (_spawnArmed)
                {
                    if (_map.TryGetTrack(pos, out var spawnTrack) && spawnTrack != null)
                    {
                        var direction = spawnTrack.GetAvailableDirections().FirstOrDefault();
                        SpawnTrain(pos, direction, _selectedPassengerWagons);
                        _spawnArmed = false;
                    }
                    return;
                }

                // Kliknięcie na depot
                var depot = _depotController.GetDepotAt(pos);
                if (depot != null && _builder.Mode != TrackBuildMode.Depot)
                {
                    _depotTrainMenu.Open(screen);
                    return;
                }

                // Budowanie
                if (_builder.Mode == TrackBuildMode.Signal)
                    PlaceSignal(pos, screen);
                else if (_builder.Mode == TrackBuildMode.Station)
                    PlaceStation(pos);
                else if (_builder.Mode == TrackBuildMode.Depot)
                    PlaceDepot(pos);
                else
                    _builder.BuildAt(pos);
            }

            // PRAWY PRZYCISK
            if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
            {
                // Depot – usuwanie
                var depot = _depotController.GetDepotAt(pos);
                if (depot != null)
                {
                    if (shift)
                        _depotController.RemoveDepot(depot);
                    return;
                }

                // Sygnały
                var signals = _signalController.GetSignalsAt(pos);
                if (signals.Count > 0)
                {
                    if (shift)
                        _signalController.RemoveSignalsAt(pos);
                    else if (signals.Count == 1)
                        _signalRadialMenu.Open(screen, signals[0]);
                    else
                        _signalSelectionMenu.Open(screen, signals);
                    return;
                }

                // Stacja – usuwanie
                var station = _stationController.GetStationAt(pos);
                if (station != null)
                {
                    if (shift)
                        _stationController.RemoveStation(station);
                    return;
                }

                // Rozjazd – menu lub usuwanie
                if (_map.TryGetTrack(pos, out var track) && track != null && track.IsJunction)
                {
                    if (shift)
                        _builder.Remove(pos);
                    else
                        _junctionRadialMenu.Open(screen, track);
                    return;
                }

                // Usuwanie toru
                if (_map.TryGetTrack(pos, out var existing) && existing != null)
                    _builder.Remove(pos);
            }
        }

        private void PlaceDepot(MapPosition pos)
        {
            var depot = new Depot($"Depot {_depotController.Depots.Count + 1}", pos);
            if (_depotController.AddDepot(depot))
                _builder.Mode = TrackBuildMode.None;
        }

        private void PlaceSignal(MapPosition pos, Vector2 screen)
        {
            if (!_map.TryGetTrack(pos, out var track) || track == null)
                return;

            var directions = track.GetAvailableDirections();
            if (directions.Count == 0)
                return;

            if (directions.Count == 1)
                _signalController.AddSignal(pos, directions[0]);
            else
                _signalDirectionMenu.Open(screen, pos, directions);
        }

        private void PlaceStation(MapPosition pos)
        {
            var origin = new MapPosition(
                pos.X - (_stationWidth - 1) / 2,
                pos.Y - (_stationHeight - 1) / 2);

            // Sprawdź, czy wszystkie pola mają tor
            for (int y = 0; y < _stationHeight; y++)
            {
                for (int x = 0; x < _stationWidth; x++)
                {
                    var cell = new MapPosition(origin.X + x, origin.Y + y);
                    if (!_map.TryGetTrack(cell, out var track) || track == null)
                        return;
                    if (_stationController.GetStationAt(cell) != null)
                        return;
                }
            }

            var station = new Station($"Stacja {_stationController.Stations.Count + 1}", origin, _stationWidth, _stationHeight);
            _stationController.AddStation(station);
        }

        private void SpawnTrain(MapPosition cell, TrackConnections direction, int passengerWagons)
        {
            if (direction == TrackConnections.None)
                direction = TrackConnections.East;

            var locomotiveParameters = new VehicleParameters(25.4f, 0.8f, 100.0f, 80000f, 1.0f);
            var wagonParameters = new VehicleParameters(25.4f, 0.8f, 100.0f, 40000f, 1.0f);

            var vehicles = new List<Vehicle>
            {
                new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters)
            };

            for (int i = 0; i < passengerWagons; i++)
                vehicles.Add(new Wagon(wagonParameters));

            var train = new Train(
                new Vector2(cell.X + 0.5f, cell.Y + 0.5f),
                direction,
                0f,
                vehicles);

            train.SetMap(_map);
            train.SetSignalController(_signalController);
            train.SetBlockController(GetBlockController());

            _trainManager.Add(train);
            SaveSchedule(train);
        }

        private BlockController GetBlockController()
        {
            return _trainManager.BlockController ?? new BlockController();
        }

        private void OnWagonRouteChanged(Wagon wagon)
        {
            foreach (var train in _trainManager.Trains)
            {
                if (!train.Composition.Vehicles.Contains(wagon))
                    continue;
                SaveSchedule(train);
                break;
            }
        }

        private void SaveSchedule(Train train)
        {
            var schedule = new TrainSchedule { TrainId = train.Id };

            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                if (train.Composition.Vehicles[i] is Wagon wagon)
                {
                    schedule.Wagons.Add(new WagonScheduleEntry
                    {
                        WagonIndex = i,
                        WagonType = wagon.WagonType,
                        StationIds = wagon.Route.StationIds.ToList()
                    });
                }
            }

            try
            {
                ScheduleStorage.Save(schedule);
            }
            catch (IOException)
            {
                // Ignoruj błędy zapisu
            }
        }

        private MapPosition ToMapPosition(Vector2 position)
            => _camera.ScreenToMap(position);

        private bool IsKeyPressed(KeyboardState keyboard, Keys key)
            => keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

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
            Vector2 screen = new(mouse.X, mouse.Y);
            MapPosition pos = _camera.ScreenToMap(screen);

            // Rysowanie świata
            _spriteBatch.Begin(transformMatrix: _camera.Transform, samplerState: SamplerState.PointClamp);

            _renderer.Draw(_spriteBatch, _camera);
            _signalRenderer.Draw(_spriteBatch, _camera);
            _stationRenderer.Draw(_spriteBatch);
            _depotRenderer.Draw(_spriteBatch);
            _trainRenderer.Draw(_spriteBatch, _trainManager);

            // Podgląd budowania
            _renderer.DrawPreview(_spriteBatch, pos, _builder.Mode, _builder.StraightHorizontal, _builder.Curve, _builder.Junction);

            if (_builder.Mode == TrackBuildMode.Station)
                _stationRenderer.DrawPreview(_spriteBatch, pos, _stationWidth, _stationHeight);
            else if (_builder.Mode == TrackBuildMode.Depot)
                _depotRenderer.DrawPreview(_spriteBatch, pos);

            _spriteBatch.End();

            // Tooltipy
            DrawStationTooltip(screen, pos);
            DrawWagonTooltip(screen);

            // Menu radialne
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (_junctionRadialMenu.IsOpen)
                _junctionRadialMenu.Draw(_spriteBatch);
            if (_signalRadialMenu.IsOpen)
                _signalRadialMenu.Draw(_spriteBatch);
            if (_signalDirectionMenu.IsOpen)
                _signalDirectionMenu.Draw(_spriteBatch);
            if (_signalSelectionMenu.IsOpen)
                _signalSelectionMenu.Draw(_spriteBatch);

            _spriteBatch.End();

            // Menu tras i depot
            var font = GetTooltipFont();
            if (font != null)
            {
                _wagonRouteMenu.SetFont(font);
                _depotTrainMenu.SetFont(font);
            }

            if (_wagonRouteMenu.IsOpen)
                _wagonRouteMenu.Draw(_spriteBatch);
            if (_depotTrainMenu.IsOpen)
                _depotTrainMenu.Draw(_spriteBatch);
        }

        private void DrawWagonTooltip(Vector2 mouse)
        {
            if (_wagonRouteMenu.IsOpen || _wagonRouteEditMode)
                return;

            var world = _camera.ScreenToWorld(mouse);
            var hit = _trainRenderer.GetVehicleAtPosition(_trainManager, world);

            if (!hit.HasValue)
                return;

            var result = hit.Value;
            var vehicle = result.train.Composition.Vehicles[result.vehicleIndex];

            if (vehicle is not Wagon wagon)
                return;

            var font = GetTooltipFont();
            var pixel = GetTooltipPixel();
            if (font == null || pixel == null)
                return;

            string route = wagon.Route.IsEmpty ? "Trasa: BRAK" : $"Trasa: {wagon.Route.StationIds.Count} st.";
            string next = wagon.Route.NextStationId.HasValue
                ? "Następny: " + wagon.Route.NextStationId.Value.ToString()[..8]
                : "Następny: -";

            string[] lines = {
                "WAGON",
                $"Pociąg: {result.train.Id.ToString()[..8]}",
                $"Wagon: {result.vehicleIndex + 1}/{result.train.Composition.Vehicles.Count}",
                $"Pasażerowie: {wagon.PassengerCount}/{wagon.PassengerCapacity}",
                route,
                next,
                "S + LPM: edytuj trasę"
            };

            DrawTooltip(mouse, lines, new Color(30, 90, 150, 230));
        }

        private void DrawStationTooltip(Vector2 mouse, MapPosition pos)
        {
            var station = _stationController.GetStationAt(pos);
            if (station == null)
                return;

            var font = GetTooltipFont();
            var pixel = GetTooltipPixel();
            if (font == null || pixel == null)
                return;

            var waiting = _stationController.Passengers.GetWaitingAt(station).ToList();

            string[] lines = {
                "STACJA",
                station.Name,
                "ID: " + station.Id.ToString()[..8],
                $"Rozmiar: {station.Width}x{station.Height}",
                "Oczekujący: " + waiting.Count,
                "Różne cele: " + waiting.Select(p => p.DestinationStation.Id).Distinct().Count(),
                "Obsługa: " + (station.PassengerServiceEnabled ? "TAK" : "NIE"),
                "Postój: " + station.DwellTimeSeconds.ToString("F1") + " s"
            };

            DrawTooltip(mouse, lines, new Color(30, 90, 150, 230));
        }

        private void DrawTooltip(Vector2 mouse, string[] lines, Color background)
        {
            var font = GetTooltipFont();
            var pixel = GetTooltipPixel();
            if (font == null || pixel == null)
                return;

            float padding = 8f;
            float lineHeight = font.MeasureString("A").Y * 0.75f + 3f;
            float width = lines.Max(x => font.MeasureString(x).X * 0.75f) + padding * 2;
            float height = lines.Length * lineHeight + padding * 2;

            Vector2 position = mouse + new Vector2(15);
            var viewport = _graphicsDevice.Viewport;

            if (position.X + width > viewport.Width)
                position.X = mouse.X - width - 15;
            if (position.Y + height > viewport.Height)
                position.Y = mouse.Y - height - 15;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Rectangle rect = new((int)position.X, (int)position.Y, (int)width, (int)height);
            _spriteBatch.Draw(pixel, rect, background);

            Vector2 text = position + new Vector2(padding);
            for (int i = 0; i < lines.Length; i++)
            {
                _spriteBatch.DrawString(
                    font,
                    lines[i],
                    text,
                    i == 0 ? Color.Yellow : Color.White,
                    0f,
                    Vector2.Zero,
                    0.75f,
                    SpriteEffects.None,
                    0f);
                text.Y += lineHeight;
            }

            _spriteBatch.End();
        }

        private SpriteFont? GetTooltipFont()
        {
            if (_tooltipFont != null)
                return _tooltipFont;

            try
            {
                _tooltipFont = _screenManager.Game.Content.Load<SpriteFont>("Arial24");
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return _tooltipFont;
        }

        private Texture2D GetTooltipPixel()
        {
            if (_tooltipPixel != null)
                return _tooltipPixel;

            _tooltipPixel = new Texture2D(_graphicsDevice, 1, 1);
            _tooltipPixel.SetData(new[] { Color.White });
            return _tooltipPixel;
        }
    }
}