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
using RailDispatchMono.Core.Screens.UI;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Screens;

public sealed class GameplayScreen
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;

    private readonly GameMap _map;
    private readonly TrackBuilder _builder;
    private readonly Camera _camera;
    private readonly TrackRenderer _renderer;

    private readonly TrainManager _trainManager;
    private readonly TrainRenderer _trainRenderer;
    private readonly TrainDebugger _trainDebugger;

    private readonly JunctionRadialMenu _junctionRadialMenu;

    // ============================================================
    // NOWE POLA DLA SEMAFORÓW
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

    public GameplayScreen(GraphicsDevice graphicsDevice)
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
        DebugManager.Log("[GAMEPLAY] Tworzę InputManager z semaforami...");
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

        SpriteFont font = content.Load<SpriteFont>("Arial24");
        _junctionRadialMenu.SetFont(font);
        _signalRadialMenu.SetFont(font);
        _signalDirectionMenu.SetFont(font);
        _signalSelectionMenu.SetFont(font);

        DebugManager.Log("[GAMEPLAY] LoadContent() - KONIEC");
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        _trainManager.Update(deltaTime);
        _trainDebugger.Update(deltaTime, _trainManager);
        _blockController?.Update(deltaTime);

        _inputManager.Update(gameTime);

        _previousScrollWheelValue = mouse.ScrollWheelValue;
        _previousMouse = mouse;
        _previousKeyboard = keyboard;
    }

    public void Draw(GameTime gameTime)
    {
        _inputManager.Draw(gameTime);
    }

    private void CreateTestTrack()
    {
        DebugManager.SetLogToFile(true, "debug_log.txt");
        DebugManager.EnableAll();

        // Dodaj testowy wpis
        DebugManager.Log("=== TEST LOG ===");
        Console.WriteLine("=== TEST CONSOLE ===");
        System.Diagnostics.Debug.WriteLine("=== TEST DEBUG ===");
        DebugManager.Log("[DEBUG] System debugowania włączony!");
        DebugManager.Log("[TRACK] Tworzę testową trasę 100x100...");

        // ============================================================
        // PARAMETRY TRASY
        // ============================================================
        const int left = 10;
        const int right = 90;
        const int top = 10;
        const int bottom = 89;

        // ============================================================
        // 1. GÓRNA PROSTA (East)
        // ============================================================
        _builder.BuildCurve(new MapPosition(left, top), CurveDirection.EastSouth);
        for (var x = left + 1; x < right; x++)
            _builder.BuildStraight(new MapPosition(x, top), horizontal: true);

        // ============================================================
        // 2. PRAWA PROSTA (South)
        // ============================================================
        _builder.BuildCurve(new MapPosition(right, top), CurveDirection.SouthWest);
        for (var y = top + 1; y < bottom; y++)
            _builder.BuildStraight(new MapPosition(right, y), horizontal: false);

        // ============================================================
        // 3. DOLNA PROSTA (West)
        // ============================================================
        _builder.BuildCurve(new MapPosition(right, bottom), CurveDirection.WestNorth);
        for (var x = right - 1; x > left; x--)
            _builder.BuildStraight(new MapPosition(x, bottom), horizontal: true);

        // ============================================================
        // 4. LEWA PROSTA (North)
        // ============================================================
        _builder.BuildCurve(new MapPosition(left, bottom), CurveDirection.NorthEast);
        for (var y = bottom - 1; y > top; y--)
            _builder.BuildStraight(new MapPosition(left, y), horizontal: false);

        DebugManager.Log("[TRACK] Testowa trasa 100x100 utworzona!");

        // ============================================================
        // 5. SEMAFORY — TYLKO 4 (po jednym na każdej prostej)
        // ============================================================
        DebugManager.Log("[SIGNAL] Dodaję 4 semafory...");

        // Górna prosta (East) — Vmax (Clear)
        _signalController.AddSignal(
            new MapPosition(50, top),  // środek górnej prostej
            TrackConnections.East,
            new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop }
        );
        var signal1 = _signalController.GetSignalAt(new MapPosition(50, top), TrackConnections.East);
        signal1?.SetAspect(SignalAspect.Clear);
        DebugManager.Log($"[SIGNAL] Dodano Vmax na (50, {top}) East");

        // Prawa prosta (South) — Speed40
        _signalController.AddSignal(
            new MapPosition(right, 50),  // środek prawej prostej
            TrackConnections.South,
            new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop }
        );
        var signal2 = _signalController.GetSignalAt(new MapPosition(right, 50), TrackConnections.South);
        signal2?.SetAspect(SignalAspect.Speed40);
        DebugManager.Log($"[SIGNAL] Dodano Speed40 na ({right}, 50) South");

        // Dolna prosta (West) — Vmax (Clear)
        _signalController.AddSignal(
            new MapPosition(50, bottom),  // środek dolnej prostej
            TrackConnections.West,
            new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop }
        );
        var signal3 = _signalController.GetSignalAt(new MapPosition(50, bottom), TrackConnections.West);
        signal3?.SetAspect(SignalAspect.Clear);
        DebugManager.Log($"[SIGNAL] Dodano Vmax na (50, {bottom}) West");

        // Lewa prosta (North) — Vmax (Clear)
        _signalController.AddSignal(
            new MapPosition(left, 50),  // środek lewej prostej
            TrackConnections.North,
            new List<SignalAspect> { SignalAspect.Clear, SignalAspect.Stop }
        );
        var signal4 = _signalController.GetSignalAt(new MapPosition(left, 50), TrackConnections.North);
        signal4?.SetAspect(SignalAspect.Clear);
        DebugManager.Log($"[SIGNAL] Dodano Vmax na ({left}, 50) North");

        // Semafor STOP na końcu (przed zakrętem na start)
        _signalController.AddSignal(
            new MapPosition(left, top + 1),
            TrackConnections.North,
            new List<SignalAspect> { SignalAspect.Stop }
        );
        DebugManager.Log($"[SIGNAL] Dodano STOP na ({left}, {top + 1}) North");

        DebugManager.Log($"[SIGNAL] Łącznie dodano {_signalController.GetAllSignals().Count} semaforów");

        // ============================================================
        // 6. BLOKI (z 5 semaforów = 4 bloki)
        // ============================================================
        if (_blockController != null)
        {
            _blockController.CreateBlocksFromSignals();
            DebugManager.Log($"[BLOCK] Utworzono {_blockController.BlockCount} bloków");
        }

        // Dodatkowe tory na dole (dla bezpieczeństwa)
        _builder.BuildStraight(new MapPosition(88, bottom), horizontal: true);
        _builder.BuildStraight(new MapPosition(87, bottom), horizontal: true);

        DebugManager.Log("[TRACK] Sprawdzam tory na dolnej prostej:");
        for (int x = 89; x >= left; x--)
        {
            if (_map.TryGetTrack(new MapPosition(x, bottom), out var track))
                DebugManager.Log($"  ({x}, {bottom}): Connections = {track.Connections}, Geometry = {track.Geometry}");
            else
                DebugManager.Log($"  ({x}, {bottom}): ❌ BRAK TORU");
        }
    }

    // ✅ TYLKO JEDNA METODA CreateTestTrain()!
    private void CreateTestTrain()
    {
        DebugManager.Log("[TRAIN] Tworzę testowy pociąg...");

        var locomotiveParameters = new VehicleParameters(
            maxSpeed: 60.4f,
            acceleration: 0.8f,
            braking: 100.0f,
            mass: 80000f,
            length: 1.0f);

        var wagonParameters = new VehicleParameters(
            maxSpeed: 60.4f,
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
            speed: 60.4f,
            vehicles: vehicles);

        train.SetMap(_map);
        train.SetSignalController(_signalController);
        train.SetBlockController(_blockController); // ✅ DODANE!

        _trainManager.Add(train);

        DebugManager.Log($"[TRAIN] Pociąg utworzony! ID: {train.Id}, Prędkość: {train.Speed}");
        DebugManager.Log($"[TRAIN] Liczba pojazdów: {train.Composition.Vehicles.Count}");
    }
}