using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.Screens.UI;
using RailDispatchMono.Core.UI.Myra;
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
    private MyraPauseView? _pauseView;
    private readonly TrainManager _trainManager;
    private readonly TrainRenderer _trainRenderer;
    private readonly TrainDebugger _trainDebugger;
    private readonly JunctionRadialMenu _junctionRadialMenu;
    private readonly SignalController _signalController;
    private readonly SignalRadialMenu _signalRadialMenu;
    private readonly SignalDirectionMenu _signalDirectionMenu;
    private readonly BlockController _blockController;
    private readonly SignalSelectionMenu _signalSelectionMenu;
    private readonly InputManager _inputManager;
    private readonly SignalRenderer _signalRenderer;
    private readonly DepotController _depotController;
    private readonly MapSaveService _mapSaveService = new();
    private readonly GameClock _clock = new();
    private readonly FloatingTextManager _floatingText = new();
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private Inputs.InputState? _inputState;
    private bool _showTrains = true;
    private bool _depotOpen;
    private bool _spawnArmed;
    private readonly Dictionary<(Guid TrainId, int WagonIndex), int> _wagonPassengerSnapshot = new();

    private const int PanelWidth = 285;
    private const float PanelTextScale = 0.75f;

    private Rectangle PanelBounds => new(
        _graphicsDevice.Viewport.Width - PanelWidth,
        0,
        PanelWidth,
        _graphicsDevice.Viewport.Height);

    public GameplayScreen(GraphicsDevice graphicsDevice, ScreenManager screenManager)
        : this(graphicsDevice, screenManager, false)
    {
    }

    public GameplayScreen(GraphicsDevice graphicsDevice, ScreenManager screenManager, bool loadExisting)
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
        _depotController = new DepotController();
        _junctionRadialMenu = new JunctionRadialMenu(_graphicsDevice, _builder);
        _inputState = screenManager.InputState;
        _signalController = new SignalController(_map);
        _blockController = new BlockController();
        _blockController.Initialize(_map, _trainManager, _signalController);
        _trainManager.Initialize(_blockController);
        _signalRadialMenu = new SignalRadialMenu(_graphicsDevice);
        _signalDirectionMenu = new SignalDirectionMenu(_graphicsDevice);
        _signalSelectionMenu = new SignalSelectionMenu(_graphicsDevice);
        _signalRenderer = new SignalRenderer(_map, _signalController);
        _signalRenderer.LoadContent(_graphicsDevice);
        _renderer.SetSignalRenderer(_signalRenderer);
        _inputManager = new InputManager(
            _graphicsDevice, _spriteBatch, _camera, _builder, _renderer, _trainManager,
            _trainRenderer, _junctionRadialMenu, _signalController, _signalRadialMenu,
            screenManager, _signalDirectionMenu, _signalSelectionMenu, _map, _depotController);
        _inputManager.DepotSelected += OnDepotSelected;

        if (!loadExisting)
        {
            CreateTestTrack();
            CreateTestTrain();
        }
    }

    private void OnDepotSelected(Depot depot)
    {
        _depotOpen = true;
        _spawnArmed = false;
        _builder.Mode = TrackBuildMode.None;
    }

    public void LoadContent(ContentManager content)
    {
        _renderer.LoadContent(_graphicsDevice);
        _trainRenderer.LoadContent(_graphicsDevice);
        _trainRenderer.SetTrainManager(_trainManager);
        _tooltipFont = content.Load<SpriteFont>("Arial24");
        _junctionRadialMenu.SetFont(_tooltipFont);
        _signalRadialMenu.SetFont(_tooltipFont);
        _signalDirectionMenu.SetFont(_tooltipFont);
        _signalSelectionMenu.SetFont(_tooltipFont);
        _floatingText.LoadContent(content);
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        SnapshotWagonPassengers();
    }

    public void LoadSavedGame() => LoadMap();

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        float realDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keyboard = Keyboard.GetState();

        if (_inputState != null && _inputState.IsPauseKeyJustPressed())
        {
            TogglePause();
            _previousKeyboard = keyboard;
            _previousMouse = Mouse.GetState();
            return;
        }

        if (_isPaused)
        {
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

        float deltaTime = _clock.Update(realDelta);
        bool hudHandled = HandleHudInput();

        if (!hudHandled)
        {
            _trainManager.Update(deltaTime);
            CapturePassengerChanges();
            _floatingText.Update(deltaTime);
            _trainDebugger.Update(deltaTime, _trainManager);
            _blockController.Update(deltaTime);
            _inputManager.Update(gameTime);
        }
        else
        {
            _inputManager.Update(gameTime);
        }

        _previousMouse = Mouse.GetState();
        _previousKeyboard = keyboard;
    }

    private void TogglePause()
    {
        if (_isPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        if (_isPaused) return;
        if (ScreenManager?.Game is not RailDispatchMonoGame game) return;

        _isPaused = true;
        _pauseView = new MyraPauseView(
            () => game.MyraUI.QueueAction(ResumeGame),
            () => game.MyraUI.QueueAction(SaveMap),
            () => game.MyraUI.QueueAction(LoadMap),
            () => game.MyraUI.QueueAction(ShowQuitConfirmation),
            _mapSaveService.Exists);
        game.MyraUI.SetRoot(_pauseView.Root);
    }

    private void ResumeGame()
    {
        if (!_isPaused) return;
        _isPaused = false;
        _pauseView = null;
        if (ScreenManager?.Game is RailDispatchMonoGame game)
            game.MyraUI.Clear();
    }

    private bool HandleHudInput()
    {
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            if (PanelBounds.Contains(mouse.Position))
            {
                int y = 86;
                if (new Rectangle(PanelBounds.X + 10, y, 120, 36).Contains(mouse.Position))
                {
                    _showTrains = true;
                    return true;
                }
                if (new Rectangle(PanelBounds.X + 135, y, 120, 36).Contains(mouse.Position))
                {
                    _showTrains = false;
                    return true;
                }
                if (_showTrains)
                {
                    y += 50;
                    foreach (Train train in _trainManager.Trains)
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
                    foreach (Station station in _trainManager.StationController.Stations)
                    {
                        if (new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34).Contains(mouse.Position))
                        {
                            _camera.Position = new Vector2(
                                station.Position.X + station.Width / 2f,
                                station.Position.Y + station.Height / 2f);
                            return true;
                        }
                        y += 38;
                    }
                }
                return true;
            }
        }

        if (PanelBounds.Contains(mouse.Position)) return true;
        return false;
    }

    private bool IsKeyPressed(KeyboardState keyboard, Keys key)
        => keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    private void CreateTestTrack()
    {
        for (int x = 8; x <= 32; x++) _builder.BuildStraight(new MapPosition(x, 20), true);
    }

    private void CreateTestTrain() => SpawnDefaultTrain(new MapPosition(12, 20), TrackConnections.East);

    private void SpawnDefaultTrain(MapPosition cell, TrackConnections direction)
    {
        if (direction == TrackConnections.None) direction = TrackConnections.East;
        VehicleParameters locomotiveParameters = new(25.4f, 0.8f, 100.0f, 80000f, 1.0f);
        VehicleParameters wagonParameters = new(25.4f, 0.8f, 100.0f, 40000f, 1.0f);
        List<Vehicle> vehicles = new()
        {
            new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters),
            new Wagon(wagonParameters), new Wagon(wagonParameters)
        };
        Train train = new(new Vector2(cell.X + 0.5f, cell.Y + 0.5f), direction, 0f, vehicles);
        train.SetMap(_map);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController);
        _trainManager.Add(train);
    }

    private void SnapshotWagonPassengers()
    {
        _wagonPassengerSnapshot.Clear();
        foreach (Train train in _trainManager.Trains)
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
                if (train.Composition.Vehicles[i] is Wagon wagon)
                    _wagonPassengerSnapshot[(train.Id, i)] = wagon.PassengerCount;
    }

    private void CapturePassengerChanges()
    {
        foreach (Train train in _trainManager.Trains)
        {
            for (int i = 0; i < train.Composition.Vehicles.Count; i++)
            {
                if (train.Composition.Vehicles[i] is not Wagon wagon) continue;
                var key = (train.Id, i);
                int previous = _wagonPassengerSnapshot.TryGetValue(key, out int value) ? value : wagon.PassengerCount;
                int current = wagon.PassengerCount;
                if (current != previous)
                {
                    int delta = current - previous;
                    (Vector2 position, float rotation) = train.GetVehicleTransform(i);
                    _floatingText.Add(delta > 0 ? $"+{delta}" : delta.ToString(), position + new Vector2(0f, -0.5f));
                }
                _wagonPassengerSnapshot[key] = current;
            }
        }
    }

    private void SaveMap()
    {
        try
        {
            _mapSaveService.Save(_map, _signalController, _trainManager.StationController, _depotController);
            _floatingText.Add("ZAPISANO", _camera.Position);
            _pauseView?.SetLoadEnabled(true);
        }
        catch (Exception ex) { DebugManager.Log("[SAVE] " + ex); }
    }

    private void LoadMap()
    {
        try
        {
            _mapSaveService.Load(_map, _signalController, _trainManager.StationController, _depotController);
            _blockController.Initialize(_map, _trainManager, _signalController);
            _trainManager.Initialize(_blockController);
            _builder.Mode = TrackBuildMode.None;
            _depotOpen = false;
            _spawnArmed = false;
            _floatingText.Add("WCZYTANO", _camera.Position);
            SnapshotWagonPassengers();
        }
        catch (Exception ex) { DebugManager.Log("[LOAD] " + ex); }
    }

    private void ShowQuitConfirmation()
    {
        MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(Localization.Resources.QuitQuestion);
        confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;
        confirmQuitMessageBox.Cancelled += ConfirmQuitMessageBoxCancelled;
        ScreenManager.AddScreen(confirmQuitMessageBox, null);
    }

    private void ConfirmQuitMessageBoxAccepted(object? sender, PlayerIndexEventArgs e) => QuitToMainMenu();
    private void ConfirmQuitMessageBoxCancelled(object? sender, PlayerIndexEventArgs e) { }
    private void QuitToMainMenu() { ResumeGame(); ScreenManager?.Game.Exit(); }

    public override void Draw(GameTime gameTime)
    {
        _inputManager.Draw(gameTime);
        _floatingText.Draw(_spriteBatch, _camera);
        DrawHud();
        DrawTooltip();
    }

    private void DrawHud()
    {
        if (_tooltipFont == null || _pixel == null) return;
        _spriteBatch.Begin();

        // The simulation clock and speed controls are now rendered exclusively by MyraGameplayView.
        DrawRect(PanelBounds, new Color(18, 18, 18, 235));
        _spriteBatch.DrawString(_tooltipFont, "OBIEKTY", new Vector2(PanelBounds.X + 10, 10), Color.White, 0f,
            Vector2.Zero, PanelTextScale, SpriteEffects.None, 0f);
        DrawButton(new Rectangle(PanelBounds.X + 10, 86, 120, 36), "POCIAGI", _showTrains, PanelTextScale);
        DrawButton(new Rectangle(PanelBounds.X + 135, 86, 120, 36), "STACJE", !_showTrains, PanelTextScale);
        int y = 136;

        if (_showTrains)
        {
            foreach (Train train in _trainManager.Trains)
            {
                string text = $"{train.Id.ToString()[..8]}  {train.Speed * 3.6f:0} km/h";
                DrawListItem(new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34), text, PanelTextScale);
                y += 38;
            }
        }
        else
        {
            foreach (Station station in _trainManager.StationController.Stations)
            {
                int waiting = _trainManager.StationController.Passengers.GetWaitingCount(station);
                DrawListItem(new Rectangle(PanelBounds.X + 10, y, PanelWidth - 20, 34),
                    $"{station.Name}  {waiting} oczek.", PanelTextScale);
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
            _spriteBatch.DrawString(_tooltipFont, "TRYB DEPOTU — kliknij, aby postawic budynek", new Vector2(20, 85),
                Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
        else if (_spawnArmed)
        {
            _spriteBatch.Begin();
            DrawRect(new Rectangle(10, 75, 370, 50), new Color(20, 20, 20, 230));
            _spriteBatch.DrawString(_tooltipFont, "Kliknij istniejacy tor, aby ustawic pociag", new Vector2(20, 85),
                Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
    }

    private void DrawTooltip()
    {
        if (_tooltipFont == null || _pixel == null) return;
        MouseState mouseState = Mouse.GetState();
        Vector2 mouseScreenPos = new(mouseState.X, mouseState.Y);
        Vector2 mouseWorldPos = _camera.ScreenToWorld(mouseScreenPos);
        var vehicleInfo = _trainRenderer.GetVehicleAtPosition(_trainManager, mouseWorldPos);
        if (!vehicleInfo.HasValue) return;

        (Train train, int vehicleIndex, Vector2 worldPos) = vehicleInfo.Value;
        Vehicle vehicle = train.Composition.Vehicles[vehicleIndex];
        bool isLoco = vehicle is Locomotive;
        Color bgColor = isLoco ? new Color(180, 30, 30, 230) : new Color(30, 30, 180, 230);
        List<string> linesList = new()
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
            var destinationGroups = wagon.Passengers.GroupBy(p => p.DestinationStation.Id)
                .Select(g => new { Destination = g.First().DestinationStation, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();
            linesList.Add(destinationGroups.Count == 0 ? "Cele: brak" : "Cele pasazerow:");
            foreach (var group in destinationGroups) linesList.Add("  " + group.Destination.Name + ": " + group.Count);
        }

        float padding = 8f;
        float lineHeight = _tooltipFont.LineSpacing * 0.65f;
        float width = linesList.Max(line => _tooltipFont.MeasureString(line).X * 0.65f) + padding * 2;
        float height = linesList.Count * lineHeight + padding * 2;
        Vector2 position = mouseScreenPos + new Vector2(15, 15);
        if (position.X + width > _graphicsDevice.Viewport.Width) position.X = mouseScreenPos.X - width - 15;
        if (position.Y + height > _graphicsDevice.Viewport.Height) position.Y = mouseScreenPos.Y - height - 15;
        _spriteBatch.Begin();
        DrawRect(new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height), bgColor);
        float y = position.Y + padding;
        foreach (string line in linesList)
        {
            _spriteBatch.DrawString(_tooltipFont, line, new Vector2(position.X + padding, y), Color.White,
                0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0f);
            y += lineHeight;
        }
        _spriteBatch.End();
    }

    private void DrawRect(Rectangle rectangle, Color color) => _spriteBatch.Draw(_pixel!, rectangle, color);
    private void DrawButton(Rectangle rectangle, string text, bool selected, float scale = 1f)
    {
        DrawRect(rectangle, selected ? new Color(70, 70, 70, 255) : new Color(40, 40, 40, 255));
        Vector2 size = _tooltipFont!.MeasureString(text) * scale;
        Vector2 position = new(rectangle.Center.X - size.X / 2f, rectangle.Center.Y - size.Y / 2f);
        _spriteBatch.DrawString(_tooltipFont, text, position, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawListItem(Rectangle rectangle, string text, float scale)
    {
        DrawRect(rectangle, new Color(35, 35, 35, 255));
        _spriteBatch.DrawString(_tooltipFont!, text, new Vector2(rectangle.X + 8, rectangle.Y + 7), Color.White,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
