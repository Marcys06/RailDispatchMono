using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Rendering;
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
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private int _previousScrollWheelValue;
    private Inputs.InputState? _inputState;

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
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_inputState != null && _inputState.IsPauseKeyJustPressed()) TogglePause();
        if (_isPaused)
        {
            _inputManager.Update(gameTime);
            return;
        }
        _trainManager.Update(deltaTime);
        _trainDebugger.Update(deltaTime, _trainManager);
        _blockController?.Update(deltaTime);
        _inputManager.Update(gameTime);
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();
        _previousScrollWheelValue = mouse.ScrollWheelValue;
        _previousMouse = mouse;
        _previousKeyboard = keyboard;
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
        DrawTooltip();
        if (_isPaused && _tooltipFont != null)
        {
            var viewport = _graphicsDevice.Viewport;
            string pauseText = "PAUZA";
            var textSize = _tooltipFont.MeasureString(pauseText);
            Vector2 position = new((viewport.Width - textSize.X) / 2,
                (viewport.Height - textSize.Y) / 2 - 50);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_tooltipFont, pauseText, position, Color.White);
            _spriteBatch.End();
        }
    }

    private void DrawTooltip()
    {
        if (_tooltipFont == null || _pixel == null || _trainManager == null) return;
        var mouseState = Mouse.GetState();
        var mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
        var mouseWorldPos = _camera.ScreenToWorld(mouseScreenPos);
        var vehicleInfo = _trainRenderer.GetVehicleAtPosition(_trainManager, mouseWorldPos);
        if (!vehicleInfo.HasValue) return;

        var (train, vehicleIndex, vehicleWorldPos) = vehicleInfo.Value;
        var vehicle = train.Composition.Vehicles[vehicleIndex];
        bool isLoco = vehicle is Locomotive;
        Color bgColor = isLoco ? new Color(180, 30, 30, 230) : new Color(30, 30, 180, 230);
        string trainId = train.Id.ToString()[..8];
        float speedMs = train.Speed;
        float speedKmh = speedMs * 3.6f;
        float mass = vehicle.Parameters.Mass;
        float length = vehicle.Parameters.Length;
        int vehicleCount = train.Composition.Vehicles.Count;

        var linesList = new List<string>
        {
            isLoco ? "LOKOMOTYWA" : "WAGON",
            "ID pociagu: " + trainId,
            "Pojazd: " + (vehicleIndex + 1) + "/" + vehicleCount,
            "Predkosc: " + speedMs.ToString("F1") + " m/s",
            "          " + speedKmh.ToString("F1") + " km/h",
            "Masa: " + mass.ToString("F0") + " kg",
            "Dlugosc: " + length.ToString("F1") + " m",
            "Kierunek: " + train.Direction
        };

        if (vehicle is Wagon wagon)
        {
            linesList.Add("Typ wagonu: " + wagon.WagonType);
            linesList.Add("Pasazerowie: " + wagon.PassengerCount + "/" + wagon.PassengerCapacity);
            linesList.Add("Wolne miejsca: " + wagon.AvailablePassengerCapacity);

            var destinationGroups = wagon.Passengers
                .GroupBy(p => p.DestinationStation.Id)
                .Select(g => new { Destination = g.First().DestinationStation, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            if (destinationGroups.Count == 0)
                linesList.Add("Cele: brak");
            else
            {
                linesList.Add("Cele pasazerow:");
                foreach (var group in destinationGroups)
                    linesList.Add("  " + group.Destination.Name + ": " + group.Count);
            }

            if (wagon.ServiceRoute.Count > 0)
                linesList.Add("Trasa wagonu: " + wagon.ServiceRoute.Count + " st.");
        }

        string[] lines = linesList.ToArray();
        float padding = 8f;
        float lineHeight = _tooltipFont.MeasureString("A").Y + 2f;
        float maxWidth = 0f;
        foreach (var line in lines) maxWidth = MathF.Max(maxWidth, _tooltipFont.MeasureString(line).X);
        float tooltipWidth = maxWidth + padding * 2f;
        float tooltipHeight = lines.Length * lineHeight + padding * 2f;
        Vector2 tooltipPos = mouseScreenPos + new Vector2(15, 15);
        var viewport = _graphicsDevice.Viewport;
        if (tooltipPos.X + tooltipWidth > viewport.Width) tooltipPos.X = mouseScreenPos.X - tooltipWidth - 15;
        if (tooltipPos.Y + tooltipHeight > viewport.Height) tooltipPos.Y = mouseScreenPos.Y - tooltipHeight - 15;

        _spriteBatch.Begin();
        Rectangle bgRect = new((int)tooltipPos.X, (int)tooltipPos.Y, (int)tooltipWidth, (int)tooltipHeight);
        _spriteBatch.Draw(_pixel, bgRect, bgColor);
        Color borderColor = isLoco ? new Color(255, 100, 100, 200) : new Color(100, 100, 255, 200);
        int borderThickness = 2;
        _spriteBatch.Draw(_pixel, new Rectangle(bgRect.X - borderThickness, bgRect.Y - borderThickness, bgRect.Width + borderThickness * 2, borderThickness), borderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(bgRect.X - borderThickness, bgRect.Y + bgRect.Height, bgRect.Width + borderThickness * 2, borderThickness), borderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(bgRect.X - borderThickness, bgRect.Y - borderThickness, borderThickness, bgRect.Height + borderThickness * 2), borderColor);
        _spriteBatch.Draw(_pixel, new Rectangle(bgRect.X + bgRect.Width, bgRect.Y - borderThickness, borderThickness, bgRect.Height + borderThickness * 2), borderColor);
        Vector2 textPos = tooltipPos + new Vector2(padding, padding);
        for (int i = 0; i < lines.Length; i++)
        {
            _spriteBatch.DrawString(_tooltipFont, lines[i], textPos, i == 0 ? Color.Yellow : Color.White);
            textPos.Y += lineHeight;
        }
        _spriteBatch.End();
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
        var vehicles = new List<Vehicle>
        {
            new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters),
            new Wagon(wagonParameters),
            new Wagon(wagonParameters)
        };
        var train = new Train(new Vector2(25.5f, 10.5f), TrackConnections.East, 25.4f, vehicles);
        train.SetMap(_map);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController);
        _trainManager.Add(train);
    }
}