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

    // ============================================================
    // POLA DLA SEMAFORÓW
    // ============================================================
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

    // ✅ REFERENCJA DO INPUTSTATE (przekazana przez ScreenManager)
    private Inputs.InputState? _inputState;

    public GameplayScreen(GraphicsDevice graphicsDevice, ScreenManager screenManager)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);

        _map = new GameMap(100, 100);
        _builder = new TrackBuilder(_map);

        _camera = new Camera
        {
            Position = new Vector2(20f, 20f),
            Zoom = 32f
        };

        _renderer = new TrackRenderer(_map);
        _trainManager = new TrainManager(_map);
        _trainRenderer = new TrainRenderer();
        _trainDebugger = new TrainDebugger(1.0f);

        _junctionRadialMenu = new JunctionRadialMenu(_graphicsDevice, _builder);

        // ✅ ZAPISZ REFERENCJĘ DO INPUTSTATE
        _inputState = screenManager.InputState;

        // ============================================================
        // TWORZENIE SIGNALCONTROLLER I MENU SEMAFORÓW
        // ============================================================
        DebugManager.Log("[GAMEPLAY] Tworzę SignalController...");
        _signalController = new SignalController(_map);
        DebugManager.Log("[GAMEPLAY] SignalController utworzony!");

        // ============================================================
        // ✅ INICJALIZACJA BLOCKCONTROLLER
        // ============================================================
        DebugManager.Log("[GAMEPLAY] Tworzę BlockController...");
        _blockController = new BlockController();
        _blockController.Initialize(_map, _trainManager, _signalController);
        DebugManager.Log("[GAMEPLAY] BlockController utworzony!");

        DebugManager.Log("[GAMEPLAY] Tworzę SignalRadialMenu...");
        _signalRadialMenu = new SignalRadialMenu(_graphicsDevice);
        DebugManager.Log("[GAMEPLAY] SignalRadialMenu utworzony!");

        DebugManager.Log("[GAMEPLAY] Tworzę SignalDirectionMenu...");
        _signalDirectionMenu = new SignalDirectionMenu(_graphicsDevice);
        DebugManager.Log("[GAMEPLAY] SignalDirectionMenu utworzony!");

        DebugManager.Log("[GAMEPLAY] Tworzę SignalSelectionMenu...");
        _signalSelectionMenu = new SignalSelectionMenu(_graphicsDevice);
        DebugManager.Log("[GAMEPLAY] SignalSelectionMenu utworzony!");

        // ============================================================
        // TWORZENIE SIGNALRENDERER
        // ============================================================
        _signalRenderer = new SignalRenderer(_map, _signalController);
        _signalRenderer.LoadContent(_graphicsDevice);
        DebugManager.Log("[GAMEPLAY] SignalRenderer utworzony!");

        // Przekaż SignalRenderer do TrackRenderer
        _renderer.SetSignalRenderer(_signalRenderer);
        DebugManager.Log("[GAMEPLAY] SignalRenderer przekazany do TrackRenderer");

        // ============================================================
        // TWORZENIE INPUTMANAGER
        // ============================================================
        _inputManager = new InputManager(
            _graphicsDevice,
            _spriteBatch,
            _camera,
            _builder,
            _renderer,
            _trainManager,
            _trainRenderer,
            _junctionRadialMenu,
            _signalController,
            _signalRadialMenu,
            screenManager,
            _signalDirectionMenu,
            _signalSelectionMenu,
            _map
        );

        DebugManager.Log("[GAMEPLAY] InputManager utworzony!");

        // ============================================================
        // SUBKRYPCJA ZDARZEŃ SEMAFORÓW (DEBUG)
        // ============================================================
        _signalRadialMenu.AspectSelected += (s, aspect) =>
        {
            DebugManager.Log($"[SIGNAL] ✅ Wybrano aspekt: {aspect}");
        };

        _signalRadialMenu.MenuClosed += (s, e) =>
        {
            DebugManager.Log("[SIGNAL] Menu aspektów zamknięte");
        };

        _signalDirectionMenu.MenuClosed += (s, e) =>
        {
            DebugManager.Log("[SIGNAL] Menu kierunków zamknięte");
        };

        _signalSelectionMenu.MenuClosed += (s, e) =>
        {
            DebugManager.Log("[SIGNAL] Menu wyboru semafora zamknięte");
        };

        CreateTestTrack();
        CreateTestTrain();

        DebugManager.Log("[GAMEPLAY] ========================================");
        DebugManager.Log("[GAMEPLAY] GameplayScreen utworzony!");
        DebugManager.Log("[GAMEPLAY] ========================================");
    }

    public void LoadContent(ContentManager content)
    {
        DebugManager.Log("[GAMEPLAY] LoadContent() - START");

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

        DebugManager.Log("[GAMEPLAY] LoadContent() - KONIEC");
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // ✅ OBSŁUGA ESC — używamy zapisanego InputState
        if (_inputState != null && _inputState.IsPauseKeyJustPressed())
        {
            TogglePause();
        }

        // Jeśli gra jest zapauzowana — nie aktualizuj symulacji
        if (_isPaused)
        {
            _inputManager.Update(gameTime);
            return;
        }

        _trainManager.Update(deltaTime);
        _trainDebugger.Update(deltaTime, _trainManager);
        _blockController?.Update(deltaTime);
        _inputManager.Update(gameTime);

        // ✅ Aktualizuj stany klawiszy dla innych potrzeb
        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();
        _previousScrollWheelValue = mouse.ScrollWheelValue;
        _previousMouse = mouse;
        _previousKeyboard = keyboard;
    }

    // ============================================================
    // PAUZA
    // ============================================================

    private void TogglePause()
    {
        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        if (_isPaused) return;

        _isPaused = true;

        // ✅ Utwórz menu pauzy
        _pauseScreen = new PauseScreen();

        // ✅ Subskrybuj eventy
        _pauseScreen.OnResume += (s, e) => ResumeGame();
        _pauseScreen.OnQuit += (s, e) => QuitToMainMenu();

        // ✅ Dodaj ekran do menedżera
        ScreenManager?.AddScreen(_pauseScreen, null);
    }

    private void ResumeGame()
    {
        if (!_isPaused) return;

        _isPaused = false;

        // ✅ Usuń menu pauzy
        if (_pauseScreen != null && ScreenManager != null)
        {
            // ✅ Sprawdź czy ekran nadal istnieje (używamy RemoveScreen które samo sprawdza)
            ScreenManager.RemoveScreen(_pauseScreen);
            _pauseScreen = null;
        }
    }

    private void QuitToMainMenu()
    {
        ResumeGame(); // Wznów grę przed wyjściem
        // TODO: Przejście do menu głównego
        // ScreenManager?.AddScreen(new MainMenuScreen(), null);
        // ExitScreen();

        // ✅ TYMCZASOWO: wyjście z gry
        ScreenManager?.Game.Exit();
    }

    // ============================================================
    // DRAW
    // ============================================================

    public override void Draw(GameTime gameTime)
    {
        _inputManager.Draw(gameTime);
        DrawTooltip();

        // ✅ NAPIS "PAUZA" - bez emoji, które mogą nie być obsługiwane przez czcionkę
        if (_isPaused && _tooltipFont != null)
        {
            var viewport = _graphicsDevice.Viewport;
            string pauseText = "PAUZA";
            var textSize = _tooltipFont.MeasureString(pauseText);

            Vector2 position = new Vector2(
                (viewport.Width - textSize.X) / 2,
                (viewport.Height - textSize.Y) / 2 - 50
            );

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_tooltipFont, pauseText, position, Color.White);
            _spriteBatch.End();
        }
    }

    // ============================================================
    // TOOLTIP
    // ============================================================

    private void DrawTooltip()
    {
        if (_tooltipFont == null || _pixel == null || _trainManager == null)
            return;

        var mouseState = Mouse.GetState();
        var mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
        var mouseWorldPos = _camera.ScreenToWorld(mouseScreenPos);

        var vehicleInfo = _trainRenderer.GetVehicleAtPosition(_trainManager, mouseWorldPos);
        if (!vehicleInfo.HasValue)
            return;

        var (train, vehicleIndex, vehicleWorldPos) = vehicleInfo.Value;
        var vehicle = train.Composition.Vehicles[vehicleIndex];
        bool isLoco = vehicle is Locomotive;

        Color bgColor = isLoco
            ? new Color(180, 30, 30, 230)
            : new Color(30, 30, 180, 230);

        string typeName = isLoco ? "LOKOMOTYWA" : "WAGON";
        string trainId = train.Id.ToString()[..8];
        float speedMs = train.Speed;
        float speedKmh = speedMs * 3.6f;
        float mass = vehicle.Parameters.Mass;
        float length = vehicle.Parameters.Length;
        int vehicleCount = train.Composition.Vehicles.Count;
        string direction = train.Direction.ToString();

        string[] lines = new string[]
        {
            typeName,
            "ID: " + trainId,
            "Predkosc: " + speedMs.ToString("F1") + " m/s",
            "          " + speedKmh.ToString("F1") + " km/h",
            "Masa: " + mass.ToString("F0") + " kg",
            "Dlugosc: " + length.ToString("F1") + " m",
            "Pojazdy: " + vehicleCount,
            "Kierunek: " + direction
        };

        float padding = 8f;
        float lineHeight = _tooltipFont.MeasureString("A").Y + 2f;
        float maxWidth = 0f;
        foreach (var line in lines)
        {
            float w = _tooltipFont.MeasureString(line).X;
            if (w > maxWidth) maxWidth = w;
        }

        float tooltipWidth = maxWidth + padding * 2f;
        float tooltipHeight = lines.Length * lineHeight + padding * 2f;

        Vector2 tooltipPos = mouseScreenPos + new Vector2(15, 15);

        var viewport = _graphicsDevice.Viewport;
        if (tooltipPos.X + tooltipWidth > viewport.Width)
            tooltipPos.X = mouseScreenPos.X - tooltipWidth - 15;
        if (tooltipPos.Y + tooltipHeight > viewport.Height)
            tooltipPos.Y = mouseScreenPos.Y - tooltipHeight - 15;

        _spriteBatch.Begin();

        Rectangle bgRect = new Rectangle(
            (int)tooltipPos.X,
            (int)tooltipPos.Y,
            (int)tooltipWidth,
            (int)tooltipHeight
        );
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
            Color lineColor = (i == 0) ? Color.Yellow : Color.White;
            _spriteBatch.DrawString(_tooltipFont, lines[i], textPos, lineColor);
            textPos.Y += lineHeight;
        }

        _spriteBatch.End();
    }

    // ============================================================
    // TEST TRACK
    // ============================================================

    private void CreateTestTrack()
    {
        DebugManager.SetLogToFile(true, "debug_log.txt");
        DebugManager.EnableAll();

        DebugManager.Log("[TRACK] Tworzę testową trasę 100x100...");

        const int left = 10;
        const int right = 90;
        const int top = 10;
        const int bottom = 89;

        _builder.BuildCurve(new MapPosition(left, top), CurveDirection.EastSouth);
        for (var x = left + 1; x < right; x++)
            _builder.BuildStraight(new MapPosition(x, top), horizontal: true);

        _builder.BuildCurve(new MapPosition(right, top), CurveDirection.SouthWest);
        for (var y = top + 1; y < bottom; y++)
            _builder.BuildStraight(new MapPosition(right, y), horizontal: false);

        _builder.BuildCurve(new MapPosition(right, bottom), CurveDirection.WestNorth);
        for (var x = right - 1; x > left; x--)
            _builder.BuildStraight(new MapPosition(x, bottom), horizontal: true);

        _builder.BuildCurve(new MapPosition(left, bottom), CurveDirection.NorthEast);
        for (var y = bottom - 1; y > top; y--)
            _builder.BuildStraight(new MapPosition(left, y), horizontal: false);

        DebugManager.Log("[TRACK] Testowa trasa 100x100 utworzona!");

        // 5. SEMAFORY — TYLKO 4
        DebugManager.Log("[SIGNAL] Dodaję 4 semafory...");

        _signalController.AddSignal(
            new MapPosition(50, top),
            TrackConnections.East,
            new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop }
        );
        var signal1 = _signalController.GetSignalAt(new MapPosition(50, top), TrackConnections.East);
        signal1?.SetAspect(SignalAspect.Clear);

        _signalController.AddSignal(
            new MapPosition(right, 50),
            TrackConnections.South,
            new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop }
        );
        var signal2 = _signalController.GetSignalAt(new MapPosition(right, 50), TrackConnections.South);
        signal2?.SetAspect(SignalAspect.Speed40);

        _signalController.AddSignal(
            new MapPosition(50, bottom),
            TrackConnections.West,
            new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop }
        );
        var signal3 = _signalController.GetSignalAt(new MapPosition(50, bottom), TrackConnections.West);
        signal3?.SetAspect(SignalAspect.Clear);

        _signalController.AddSignal(
            new MapPosition(left, 50),
            TrackConnections.North,
            new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop }
        );
        var signal4 = _signalController.GetSignalAt(new MapPosition(left, 50), TrackConnections.North);
        signal4?.SetAspect(SignalAspect.Clear);

        _signalController.AddSignal(
            new MapPosition(left, top + 1),
            TrackConnections.North,
            new List<SignalAspect> { SignalAspect.Stop }
        );

        DebugManager.Log($"[SIGNAL] Łącznie dodano {_signalController.GetAllSignals().Count} semaforów");

        if (_blockController != null)
        {
            _blockController.CreateBlocksFromSignals();
            DebugManager.Log($"[BLOCK] Utworzono {_blockController.BlockCount} bloków");
        }

        _builder.BuildStraight(new MapPosition(88, bottom), horizontal: true);
        _builder.BuildStraight(new MapPosition(87, bottom), horizontal: true);
    }

    // ============================================================
    // TEST TRAIN
    // ============================================================

    private void CreateTestTrain()
    {
        DebugManager.Log("[TRAIN] Tworzę testowy pociąg...");

        var locomotiveParameters = new VehicleParameters(
            maxSpeed: 25.4f,
            acceleration: 0.8f,
            braking: 100.0f,
            mass: 80000f,
            length: 1.0f);

        var wagonParameters = new VehicleParameters(
            maxSpeed: 25.4f,
            acceleration: 0.8f,
            braking: 100.0f,
            mass: 40000f,
            length: 1.0f);

        var vehicles = new List<Vehicle>
        {
            new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters),
            new Wagon(wagonParameters),
            new Wagon(wagonParameters)
        };

        var train = new Train(
            spawnPosition: new Vector2(25.5f, 10.5f),
            initialDirection: TrackConnections.East,
            speed: 25.4f,
            vehicles: vehicles);

        train.SetMap(_map);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController);

        _trainManager.Add(train);

        DebugManager.Log($"[TRAIN] Pociąg utworzony! ID: {train.Id}, Prędkość: {train.Speed}");
        DebugManager.Log($"[TRAIN] Liczba pojazdów: {train.Composition.Vehicles.Count}");
    }
}