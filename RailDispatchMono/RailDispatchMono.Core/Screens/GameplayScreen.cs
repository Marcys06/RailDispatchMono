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
    private readonly Dictionary<(Guid TrainId, int WagonIndex), int> _wagonPassengerSnapshot = new();

    private const int PanelWidth = 0;

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
            CreateTestTrack();
    }

    private void OnDepotSelected(Depot depot)
    {
        _builder.Mode = TrackBuildMode.None;
        _spawnArmed = false;
        if (ScreenManager?.Game is RailDispatchMonoGame game)
        {
            var depotScreen = new DepotScreen(_trainManager, _signalController, _blockController, depot, game.MyraUI);
            ScreenManager.AddScreen(depotScreen, null);
        }
    }

    private bool _spawnArmed;

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

    private bool HandleHudInput() => false;

    private bool IsKeyPressed(KeyboardState keyboard, Keys key)
        => keyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    private void CreateTestTrack()
    {
        for (int x = 8; x <= 32; x++)
            _builder.BuildStraight(new MapPosition(x, 20), true);
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
        if (_builder.Mode == TrackBuildMode.Depot)
        {
            if (_tooltipFont == null || _pixel == null) return;
            _spriteBatch.Begin();
            DrawRect(new Rectangle(10, 75, 380, 50), new Color(20, 20, 20, 230));
            _spriteBatch.DrawString(_tooltipFont, "TRYB DEPOTU — kliknij, aby postawić budynek", new Vector2(20, 85),
                Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            _spriteBatch.End();
        }
        else if (_spawnArmed)
        {
            if (_tooltipFont == null || _pixel == null) return;
            _spriteBatch.Begin();
            DrawRect(new Rectangle(10, 75, 370, 50), new Color(20, 20, 20, 230));
            _spriteBatch.DrawString(_tooltipFont, "Kliknij istniejący tor, aby ustawić pociąg", new Vector2(20, 85),
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
            "ID pociągu: " + train.Id.ToString()[..8],
            "Pojazd: " + (vehicleIndex + 1) + "/" + train.Composition.Vehicles.Count,
            "Prędkość: " + (train.Speed * 3.6f).ToString("F1") + " km/h",
            "Docelowa: " + (train.EffectiveTargetSpeed * 3.6f).ToString("F1") + " km/h",
            "Vmax składu: " + (train.MaxSpeed * 3.6f).ToString("F1") + " km/h",
            "Masa: " + vehicle.Parameters.MassTons.ToString("F1") + " t",
            "Długość: " + vehicle.Parameters.LengthMeters.ToString("F1") + " m",
            "Kierunek: " + train.Direction
        };
        if (vehicle is Wagon wagon)
        {
            linesList.Add("Typ wagonu: " + wagon.WagonType);
            linesList.Add("Pasażerowie: " + wagon.PassengerCount + "/" + wagon.PassengerCapacity);
            linesList.Add("Wolne miejsca: " + wagon.AvailablePassengerCapacity);
            var destinationGroups = wagon.Passengers.GroupBy(p => p.DestinationStation.Id)
                .Select(g => new { Destination = g.First().DestinationStation, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(5).ToList();
            linesList.Add(destinationGroups.Count == 0 ? "Cele: brak" : "Cele pasażerów:");
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
}
