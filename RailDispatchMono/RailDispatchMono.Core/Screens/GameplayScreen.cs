using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.Screens.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.Screens;

public sealed class GameplayScreen : GameScreen
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private SpriteFont? _tooltipFont;
    private Texture2D? _pixel;
    private readonly GameMap _map;
    private readonly TrackBuilder _builder;
    private readonly Camera _camera;
    private readonly TrackRenderer _renderer;
    private bool _isPaused;
    private PauseScreen? _pauseScreen;
    private readonly TrainManager _trainManager;
    private readonly TrainRenderer _trainRenderer;
    private readonly TrainDebugger _trainDebugger;
    private readonly JunctionRadialMenu _junctionRadialMenu;
    private SignalController _signalController;
    private SignalRadialMenu _signalRadialMenu;
    private SignalDirectionMenu _signalDirectionMenu;
    private BlockController _blockController;
    private SignalSelectionMenu _signalSelectionMenu;
    private InputManager _inputManager;
    private SignalRenderer _signalRenderer;
    private readonly GameClock _clock = new();
    private readonly FloatingTextManager _floatingText = new();
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private Inputs.InputState? _inputState;
    private bool _objectPanelOpen;
    private bool _showTrains = true;
    private bool _depotOpen;
    private bool _spawnArmed;
    private readonly Dictionary<(Guid TrainId, int WagonIndex), int> _wagonPassengerSnapshot = new();

    private const int PanelWidth = 285;
    private const float PanelTextScale = 0.75f;
    private Rectangle PanelBounds => new(_graphicsDevice.Viewport.Width - PanelWidth, 0, PanelWidth, _graphicsDevice.Viewport.Height);

    public GameplayScreen(GraphicsDevice graphicsDevice, ScreenManager screenManager)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _map = new GameMap(100, 100);
        _builder = new TrackBuilder(_map);
        _camera = new Camera { Position = new Vector2(20f, 20f), Zoom = 32f };
        _renderer = new TrackRenderer(_map);
        _trainManager = new TrainManager(_map);
        _trainRenderer = new TrainRenderer();
        _trainDebugger = new TrainDebugger(1.0f);
        _junctionRadialMenu = new JunctionRadialMenu(_graphicsDevice, _builder);
        _inputState = screenManager.InputState;

        _signalController = new SignalController(_map);
        _blockController = new BlockController();
        _blockController.Initialize(_map, _trainManager, _signalController);
        _signalRadialMenu = new SignalRadialMenu(_graphicsDevice);
        _signalDirectionMenu = new SignalDirectionMenu(_graphicsDevice);
        _signalSelectionMenu = new SignalSelectionMenu(_graphicsDevice);
        _signalRenderer = new SignalRenderer(_map, _signalController);
        _signalRenderer.LoadContent(_graphicsDevice);
        _renderer.SetSignalRenderer(_signalRenderer);

        _inputManager = new InputManager(
            _graphicsDevice, _spriteBatch, _camera, _builder, _renderer,
            _trainManager, _trainRenderer, _junctionRadialMenu, _signalController,
            _signalRadialMenu, screenManager, _signalDirectionMenu,
            _signalSelectionMenu, _map);

        CreateTestTrack();
        CreateTestTrain();
    }

    public void LoadContent(ContentManager content)
    {
        _renderer.LoadContent(_graphicsDevice);
        _trainRenderer.LoadContent(_graphicsDevice);
        _trainRenderer.SetTrainManager(_trainManager);
        _tooltipFont = content.Load<SpriteFont>("Arial24");
        SpriteFont font = content.Load<SpriteFont>("Arial24");
        _junctionRadialMenu.SetFont(font);
        _signalRadialMenu.SetFont(font);
        _signalDirectionMenu.SetFont(font);
        _signalSelectionMenu.SetFont(font);
        _floatingText.LoadContent(content);
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        SnapshotWagonPassengers();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        float realDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboard = Keyboard.GetState();
        if (_inputState != null && _inputState.IsPauseKeyJustPressed()) TogglePause();
        if (_isPaused)
        {
            _inputManager.Update(gameTime);
            _previousKeyboard = keyboard;
            _previousMouse = Mouse.GetState();
            return;
        }

        if (IsKeyPressed(keyboard, Keys.D9) || IsKeyPressed(keyboard, Keys.NumPad9))
        {
            _builder.Mode = TrackBuildMode.Depot;
            _depotOpen = false;
            _spawnArmed = false;
        }

        if (HandleHudInput())
        {
            _previousKeyboard = keyboard;
            _previousMouse = Mouse.GetState();
            return;
        }

        float deltaTime = _clock.Update(realDelta);
        _trainManager.Update(deltaTime);
        CapturePassengerChanges();
        _floatingText.Update(deltaTime);
        _trainDebugger.Update(deltaTime, _trainManager);
        _blockController?.Update(deltaTime);
        _inputManager.Update(gameTime);
        _previousMouse = Mouse.GetState();
        _previousKeyboard = keyboard;
    }

    private bool HandleHudInput()
    {
        var mouse = Mouse.GetState();
        if (mouse.LeftButton != ButtonState.Pressed || _previousMouse.LeftButton == ButtonState.Pressed)
            return false;

        if (new Rectangle(16, 36, 50, 22).Contains(mouse.Position))
        {
            _clock.SetSpeed(1f);
            return true;
        }
        if (new Rectangle(70, 36, 50, 22).Contains(mouse.Position))
        {
            _clock.SetSpeed(2f);
            return true;
        }
        if (new Rectangle(124, 36, 50, 22).Contains(mouse.Position))
        {
            _clock.SetSpeed(5f);
            return true;
        }

        if (_depotOpen)
        {
            Rectangle spawnButton = new(80, 150, 250, 50);
            Rectangle closeButton = new(80, 215, 250, 42);
            if (spawnButton.Contains(mouse.Position))
            {
                _spawnArmed = true;
                _depotOpen = false;
                return true;
            }
            if (closeButton.Contains(mouse.Position))
            {
                _depotOpen = false;
                return true;
            }
            return true;
        }

        if (_spawnArmed)
        {
            if (PanelBounds.Contains(mouse.Position)) return true;
            var cell = _camera.ScreenToMap(new Vector2(mouse.X, mouse.Y));
            if (_map.TryGetTrack(cell, out var track) && track != null)
            {
                SpawnDefaultTrain(cell, track.GetAvailableDirections().FirstOrDefault());
                _spawnArmed = false;
            }
            return true;
        }

        if (PanelBounds.Contains(mouse.Position))
        {
            int y = 86;
            if (new Rectangle(PanelBounds.X + 10, y, 120, 36).Contains(mouse.Position)) { _showTrains = true; return true; }
            if (new Rectangle(PanelBounds.X + 135, y, 120, 36).Contains(mouse.Position)) { _showTrains = false; return true; }
            if (_showTrains)
            {
                y += 50;
                foreach (var train in _trainManager.Trains)
                {
                    if (new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34).Contains(mouse.Position))
                    {
                        _camera.Position = train.Position;
                        return true;
                    }
                    y += 38;
                }
            }
            else
            {
                y += 50;
                foreach (var station in _trainManager.StationController.Stations)
                {
                    if (new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34).Contains(mouse.Position))
                    {
                        _camera.Position = new Vector2(station.Position.X + station.Width / 2f, station.Position.Y + station.Height / 2f);
                        return true;
                    }
                    y += 38;
                }
            }
            return true;
        }
        return false;
    }

    private bool IsKeyPressed(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    private void SpawnDefaultTrain(MapPosition cell, TrackConnections direction)
    {
        if (direction == TrackConnections.None) direction = TrackConnections.East;
        var locomotiveParameters = new VehicleParameters(25.4f, 0.8f, 100.0f, 80000f, 1.0f);
        var wagonParameters = new VehicleParameters(25.4f, 0.8f, 100.0f, 40000f, 1.0f);
        var vehicles = new List<Vehicle>
        {
            new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters),
            new Wagon(wagonParameters),
            new Wagon(wagonParameters)
        };
        var train = new Train(new Vector2(cell.X + 0.5f, cell.Y + 0.5f), direction, 0f, vehicles);
        train.SetMap(_map);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController);
        _trainManager.Add(train);
    }

    private void SnapshotWagonPassengers()
    {
        _wagonPassengerSnapshot.Clear();
        foreach (var train in _trainManager.Trains)
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
                if (train.Composition.Vehicles[i] is Wagon wagon)
                    _wagonPassengerSnapshot[(train.Id, i)] = wagon.PassengerCount;
    }

    private void CapturePassengerChanges()
    {
        foreach (var train in _trainManager.Trains)
        {
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                if (train.Composition.Vehicles[i] is not Wagon wagon) continue;
                var key = (train.Id, i);
                int previous = _wagonPassengerSnapshot.TryGetValue(key, out var value) ? value : wagon.PassengerCount;
                int current = wagon.PassengerCount;
                if (current != previous)
                {
                    int delta = current - previous;
                    var transform = train.GetVehicleTransform(i);
                    _floatingText.Add(delta > 0 ? $"+{delta}" : delta.ToString(), transform.Position + new Vector2(0f, -0.5f));
                }
                _wagonPassengerSnapshot[key] = current;
            }
        }
    }

    private void TogglePause()
    {
        if (_isPaused) ResumeGame(); else PauseGame();
    }

    private void PauseGame()
    {
        if (_isPaused) return;
        _isPaused = true;
        _pauseScreen = new PauseScreen();
        _pauseScreen.OnResume += (s, e) => ResumeGame();
        _pauseScreen.OnQuit += (s, e) => QuitToMainMenu();
        ScreenManager?.AddScreen(_pauseScreen, null);
    }

    private void ResumeGame()
    {
        if (!_isPaused) return;
        _isPaused = false;
        if (_pauseScreen != null && ScreenManager != null)
        {
            ScreenManager.RemoveScreen(_pauseScreen);
            _pauseScreen = null;
        }
    }

    private void QuitToMainMenu()
    {
        ResumeGame();
        ScreenManager?.Game.Exit();
    }

    public override void Draw(GameTime gameTime)
    {
        _inputManager.Draw(gameTime);
        DrawHud();
        DrawTooltip();
        if (_isPaused && _tooltipFont != null)
        {
            var viewport = _graphicsDevice.Viewport;
            string pauseText = "PAUZA";
            var textSize = _tooltipFont.MeasureString(pauseText);
            Vector2 position = new((viewport.Width - textSize.X) / 2, (viewport.Height - textSize.Y) / 2 - 50);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_tooltipFont, pauseText, position, Color.White);
            _spriteBatch.End();
        }
    }

    private void DrawHud()
    {
        if (_tooltipFont == null || _pixel == null) return;
        _spriteBatch.Begin();
        DrawRect(new Rectangle(0, 0, 210, 65), new Color(20, 20, 20, 220));
        _spriteBatch.DrawString(_tooltipFont, _clock.DisplayTime, new Vector2(16, 5), Color.White);
        _spriteBatch.DrawString(_tooltipFont, $"x{_clock.SimulationSpeed:0}", new Vector2(130, 5), Color.Yellow);
        DrawButton(new Rectangle(16, 36, 50, 22), "x1", _clock.SimulationSpeed == 1f);
        DrawButton(new Rectangle(70, 36, 50, 22), "x2", _clock.SimulationSpeed == 2f);
        DrawButton(new Rectangle(124, 36, 50, 22), "x5", _clock.SimulationSpeed == 5f);

        DrawRect(PanelBounds, new Color(18, 18, 18, 235));
        _spriteBatch.DrawString(_tooltipFont, "OBIEKTY", new Vector2(PanelBounds.X + 10, 10), Color.White, 0f, Vector2.Zero, PanelTextScale, SpriteEffects.None, 0f);
        DrawButton(new Rectangle(PanelBounds.X + 10, 86, 120, 36), "POCIAGI", _showTrains, PanelTextScale);
        DrawButton(new Rectangle(PanelBounds.X + 135, 86, 120, 36), "STACJE", !_showTrains, PanelTextScale);

        int y = 136;
        if (_showTrains)
        {
            foreach (var train in _trainManager.Trains)
            {
                string text = $"{train.Id.ToString()[..8]}  {train.Speed * 3.6f:0} km/h";
                DrawListItem(new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34), text, PanelTextScale);
                y += 38;
            }
        }
        else
        {
            foreach (var station in _trainManager.StationController.Stations)
            {
                string text = $"{station.Name}  {_trainManager.StationController.Passengers.GetWaitingCount(station)} oczek.";
                DrawListItem(new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34), text, PanelTextScale);
                y += 38;
            }
        }
        _spriteBatch.End();

        if (_depotOpen)
        {
            _spriteBatch.Begin();
            DrawRect(new Rectangle(50, 95, 310, 185), new Color(20, 20, 20, 245));
            _spriteBatch.DrawString(_tooltipFont, "DEPOT", new Vector2(80, 110), Color.Yellow);
            _spriteBatch.DrawString(_tooltipFont, "Lokomotywa + 2 wagony", new Vector2(80, 130), Color.White);
            DrawButton(new Rectangle(80, 150, 250, 50), "WYBIERZ I USTAW", false);
            DrawButton(new Rectangle(80, 215, 250, 42), "ZAMKNIJ", false);
            _spriteBatch.End();
        }
        else if (_builder.Mode == TrackBuildMode.Depot)
        {
            _spriteBatch.Begin();
            DrawRect(new Rectangle(10, 75, 380, 50), new Color(20, 20, 20, 230));
            _spriteBatch.DrawString(_tooltipFont, "TRYB DEPOTU — kliknij, aby postawić budynek", new Vector2(20, 85), Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
        else if (_spawnArmed)
        {
            _spriteBatch.Begin();
            DrawRect(new Rectangle(10, 75, 370, 50), new Color(20, 20, 20, 230));
            _spriteBatch.DrawString(_tooltipFont, "Kliknij istniejący tor, aby ustawić pociąg", new Vector2(20, 85), Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
    }

    private void DrawTooltip()
    {
        if (_tooltipFont == null || _pixel == null) return;
        var mouseState = Mouse.GetState();
        var mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
        var mouseWorldPos = _camera.ScreenToWorld(mouseScreenPos);
        var vehicleInfo = _trainRenderer.GetVehicleAtPosition(_trainManager, mouseWorldPos);
        if (!vehicleInfo.HasValue) return;

        var (train, vehicleIndex, _) = vehicleInfo.Value;
        var vehicle = train.Composition.Vehicles[vehicleIndex];
        bool isLoco = vehicle is Locomotive;
        Color bgColor = isLoco ? new Color(180, 30, 30, 230) : new Color(30, 30, 180, 230);
        var linesList = new List<string>
        {
            isLoco ? "LOKOMOTYWA" : "WAGON",
            "ID pociagu: " + train.Id.ToString()[..8],
            "Pojazd: " + (vehicleIndex + 1) + "/" + train.Composition.Vehicles.Count,
            "Predkosc: " + (train.Speed * 3.6f).ToString("F1") + " km/h",
            "Docelowa: " + (train.EffectiveTargetSpeed * 3.6f).ToString("F1") + " km/h",
            "Vmax skladu: " + (train.MaxSpeed * 3.6f).ToString("F1") + " km/h",
            "Masa: " + vehicle.Parameters.Mass.ToString("F0") + " kg",
            "Dlugosc: " + vehicle.Parameters.Length.ToString("F1") + " m",
            "Kierunek: " + train.Direction
        };
        if (vehicle is Wagon wagon)
        {
            linesList.Add("Typ wagonu: " + wagon.WagonType);
            linesList.Add("Pasazerowie: " + wagon.PassengerCount + "/" + wagon.PassengerCapacity);
            linesList.Add("Wolne miejsca: " + wagon.AvailablePassengerCapacity);
            var destinationGroups = wagon.Passengers.GroupBy(p => p.DestinationStation.Id).Select(g => new { Destination = g.First().DestinationStation, Count = g.Count() }).OrderByDescending(x => x.Count).Take(5).ToList();
            linesList.Add(destinationGroups.Count == 0 ? "Cele: brak" : "Cele pasazerow:");
            foreach (var group in destinationGroups) linesList.Add("  " + group.Destination.Name + ": " + group.Count);
        }

        float padding = 8f, lineHeight = _tooltipFont.MeasureString("A").Y + 2f, maxWidth = 0f;
        foreach (var line in linesList) maxWidth = MathF.Max(maxWidth, _tooltipFont.MeasureString(line).X);
        float tooltipWidth = maxWidth + padding * 2f, tooltipHeight = linesList.Count * lineHeight + padding * 2f;
        Vector2 tooltipPos = mouseScreenPos + new Vector2(15, 15);
        var viewport = _graphicsDevice.Viewport;
        if (tooltipPos.X + tooltipWidth > viewport.Width) tooltipPos.X = mouseScreenPos.X - tooltipWidth - 15;
        if (tooltipPos.Y + tooltipHeight > viewport.Height) tooltipPos.Y = mouseScreenPos.Y - tooltipHeight - 15;
        _spriteBatch.Begin();
        Rectangle rect = new((int)tooltipPos.X, (int)tooltipPos.Y, (int)tooltipWidth, (int)tooltipHeight);
        _spriteBatch.Draw(_pixel, rect, bgColor);
        Vector2 textPos = tooltipPos + new Vector2(padding);
        for (int i = 0; i < linesList.Count; i++) { _spriteBatch.DrawString(_tooltipFont, linesList[i], textPos, i == 0 ? Color.Yellow : Color.White); textPos.Y += lineHeight; }
        _spriteBatch.End();
    }

    private void DrawRect(Rectangle rect, Color color) => _spriteBatch.Draw(_pixel!, rect, color);

    private void DrawButton(Rectangle rect, string text, bool active, float scale = 1f)
    {
        DrawRect(rect, active ? new Color(70, 90, 130, 255) : new Color(45, 45, 45, 255));
        var size = _tooltipFont!.MeasureString(text) * scale;
        _spriteBatch.DrawString(_tooltipFont, text, new Vector2(rect.Center.X - size.X / 2f, rect.Center.Y - size.Y / 2f), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawListItem(Rectangle rect, string text, float scale = 1f)
    {
        DrawRect(rect, new Color(38, 38, 38, 255));
        _spriteBatch.DrawString(_tooltipFont!, text, new Vector2(rect.X + 7, rect.Y + 7), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void CreateTestTrack()
    {
        DebugManager.SetLogToFile(true, "debug_log.txt");
        DebugManager.EnableAll();
        const int left = 10, right = 90, top = 10, bottom = 89;
        _builder.BuildCurve(new MapPosition(left, top), CurveDirection.EastSouth);
        for (var x = left + 1; x < right; x++) _builder.BuildStraight(new MapPosition(x, top), true);
        _builder.BuildCurve(new MapPosition(right, top), CurveDirection.SouthWest);
        for (var y = top + 1; y < bottom; y++) _builder.BuildStraight(new MapPosition(right, y), false);
        _builder.BuildCurve(new MapPosition(right, bottom), CurveDirection.WestNorth);
        for (var x = right - 1; x > left; x--) _builder.BuildStraight(new MapPosition(x, bottom), true);
        _builder.BuildCurve(new MapPosition(left, bottom), CurveDirection.NorthEast);
        for (var y = bottom - 1; y > top; y--) _builder.BuildStraight(new MapPosition(left, y), false);
        _signalController.AddSignal(new MapPosition(50, top), TrackConnections.East, new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop });
        _signalController.GetSignalAt(new MapPosition(50, top), TrackConnections.East)?.SetAspect(SignalAspect.Clear);
        _signalController.AddSignal(new MapPosition(right, 50), TrackConnections.South, new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop });
        _signalController.GetSignalAt(new MapPosition(right, 50), TrackConnections.South)?.SetAspect(SignalAspect.Speed40);
        _signalController.AddSignal(new MapPosition(50, bottom), TrackConnections.West, new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop });
        _signalController.GetSignalAt(new MapPosition(50, bottom), TrackConnections.West)?.SetAspect(SignalAspect.Clear);
        _signalController.AddSignal(new MapPosition(left, 50), TrackConnections.North, new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop });
        _signalController.GetSignalAt(new MapPosition(left, 50), TrackConnections.North)?.SetAspect(SignalAspect.Clear);
        _signalController.AddSignal(new MapPosition(left, top + 1), TrackConnections.North, new List<SignalAspect> { SignalAspect.Stop });
        _blockController.CreateBlocksFromSignals();
        _builder.BuildStraight(new MapPosition(88, bottom), true);
        _builder.BuildStraight(new MapPosition(87, bottom), true);
    }

    private void CreateTestTrain()
    {
        var locomotiveParameters = new VehicleParameters(25.4f, 0.8f, 100.0f, 80000f, 1.0f);
        var wagonParameters = new VehicleParameters(25.4f, 0.8f, 100.0f, 40000f, 1.0f);
        var vehicles = new List<Vehicle> { new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters), new Wagon(wagonParameters), new Wagon(wagonParameters) };
        var train = new Train(new Vector2(25.5f, 10.5f), TrackConnections.East, 25.4f, vehicles);
        train.SetMap(_map);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController);
        _trainManager.Add(train);
    }
}
