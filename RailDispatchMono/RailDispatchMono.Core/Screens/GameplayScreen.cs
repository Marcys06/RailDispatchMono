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
using Debug = System.Diagnostics.Debug;

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
        Debug.WriteLine("[GAMEPLAY] Tworzę SignalController...");
        _signalController = new SignalController(_map);
        Debug.WriteLine("[GAMEPLAY] SignalController utworzony!");

        // ============================================================
        // ✅ INICJALIZACJA BLOCKCONTROLLER
        // ============================================================
        Debug.WriteLine("[GAMEPLAY] Tworzę BlockController...");
        _blockController = new BlockController();
        _blockController.Initialize(_map, _trainManager, _signalController);
        Debug.WriteLine("[GAMEPLAY] BlockController utworzony!");

        Debug.WriteLine("[GAMEPLAY] Tworzę SignalRadialMenu...");
        _signalRadialMenu = new SignalRadialMenu(_graphicsDevice);
        Debug.WriteLine("[GAMEPLAY] SignalRadialMenu utworzony!");

        Debug.WriteLine("[GAMEPLAY] Tworzę SignalDirectionMenu...");
        _signalDirectionMenu = new SignalDirectionMenu(_graphicsDevice);
        Debug.WriteLine("[GAMEPLAY] SignalDirectionMenu utworzony!");

        Debug.WriteLine("[GAMEPLAY] Tworzę SignalSelectionMenu...");
        _signalSelectionMenu = new SignalSelectionMenu(_graphicsDevice);
        Debug.WriteLine("[GAMEPLAY] SignalSelectionMenu utworzony!");

        // ============================================================
        // TWORZENIE SIGNALRENDERER
        // ============================================================
        _signalRenderer = new SignalRenderer(_map, _signalController);
        _signalRenderer.LoadContent(_graphicsDevice);
        Debug.WriteLine("[GAMEPLAY] SignalRenderer utworzony!");

        // Przekaż SignalRenderer do TrackRenderer
        _renderer.SetSignalRenderer(_signalRenderer);
        Debug.WriteLine("[GAMEPLAY] SignalRenderer przekazany do TrackRenderer");

        // ============================================================
        // TWORZENIE INPUTMANAGER
        // ============================================================
        Debug.WriteLine("[GAMEPLAY] Tworzę InputManager z semaforami...");
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
        Debug.WriteLine("[GAMEPLAY] InputManager utworzony!");

        // ============================================================
        // SUBKRYPCJA ZDARZEŃ SEMAFORÓW (DEBUG)
        // ============================================================
        _signalRadialMenu.AspectSelected += (s, aspect) =>
        {
            Debug.WriteLine($"[SIGNAL] ✅ Wybrano aspekt: {aspect}");
        };

        _signalRadialMenu.MenuClosed += (s, e) =>
        {
            Debug.WriteLine("[SIGNAL] Menu aspektów zamknięte");
        };

        _signalDirectionMenu.MenuClosed += (s, e) =>
        {
            Debug.WriteLine("[SIGNAL] Menu kierunków zamknięte");
        };

        _signalSelectionMenu.MenuClosed += (s, e) =>
        {
            Debug.WriteLine("[SIGNAL] Menu wyboru semafora zamknięte");
        };

        CreateTestTrack();
        CreateTestTrain();

        Debug.WriteLine("[GAMEPLAY] ========================================");
        Debug.WriteLine("[GAMEPLAY] GameplayScreen utworzony!");
        Debug.WriteLine("[GAMEPLAY] ========================================");
    }

    public void LoadContent(ContentManager content)
    {
        Debug.WriteLine("[GAMEPLAY] LoadContent() - START");

        _renderer.LoadContent(_graphicsDevice);
        _trainRenderer.LoadContent(_graphicsDevice);

        SpriteFont font = content.Load<SpriteFont>("Arial24");
        _junctionRadialMenu.SetFont(font);
        _signalRadialMenu.SetFont(font);
        _signalDirectionMenu.SetFont(font);
        _signalSelectionMenu.SetFont(font);

        Debug.WriteLine("[GAMEPLAY] LoadContent() - KONIEC");
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        _trainManager.Update(deltaTime);
        _trainDebugger.Update(deltaTime, _trainManager);

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
        Debug.WriteLine("[TRACK] Tworzę testową trasę 100x100 (bez zwrotnic)...");

        // ============================================================
        // PARAMETRY TRASY
        // ============================================================
        const int left = 10;
        const int right = 90;
        const int top = 10;
        const int bottom = 89;

        // ============================================================
        // 1. GÓRNA PROSTA
        // ============================================================
        _builder.BuildCurve(new MapPosition(left, top), CurveDirection.EastSouth);

        for (var x = left + 1; x < right; x++)
        {
            _builder.BuildStraight(new MapPosition(x, top), horizontal: true);
        }

        // ============================================================
        // 2. PRAWA PROSTA
        // ============================================================
        _builder.BuildCurve(new MapPosition(right, top), CurveDirection.SouthWest);

        for (var y = top + 1; y < bottom; y++)
        {
            _builder.BuildStraight(new MapPosition(right, y), horizontal: false);
        }

        // ============================================================
        // 3. DOLNA PROSTA
        // ============================================================
        _builder.BuildCurve(new MapPosition(right, bottom), CurveDirection.WestNorth);

        for (var x = right - 1; x > left; x--)
        {
            _builder.BuildStraight(new MapPosition(x, bottom), horizontal: true);
        }

        // ============================================================
        // 4. LEWA PROSTA
        // ============================================================
        _builder.BuildCurve(new MapPosition(left, bottom), CurveDirection.NorthEast);

        for (var y = bottom - 1; y > top; y--)
        {
            _builder.BuildStraight(new MapPosition(left, y), horizontal: false);
        }

        Debug.WriteLine("[TRACK] Testowa trasa 100x100 utworzona!");

        // ============================================================
        // 5. SEMAFORY (co 10 komórek) - WSZYSTKIE Vmax40
        // ============================================================
        Debug.WriteLine("[SIGNAL] Dodaję semafory na trasie 100x100 (Vmax40)...");

        // Górna prosta (East) - Speed40
        for (int x = left + 5; x < right; x += 10)
        {
            _signalController.AddSignal(
                new MapPosition(x, top),
                TrackConnections.East,
                new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop }
            );

            // ✅ Ustaw początkowy aspekt na Speed40
            var signal = _signalController.GetSignalAt(new MapPosition(x, top), TrackConnections.East);
            if (signal != null)
            {
                signal.SetAspect(SignalAspect.Speed40);
            }

            Debug.WriteLine($"[SIGNAL] Dodano semafor na ({x}, {top}) East");
        }

        // Prawa prosta (South) - Speed40
        for (int y = top + 5; y < bottom; y += 10)
        {
            _signalController.AddSignal(
                new MapPosition(right, y),
                TrackConnections.South,
                new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop }
            );

            // ✅ Ustaw początkowy aspekt na Speed40
            var signal = _signalController.GetSignalAt(new MapPosition(right, y), TrackConnections.South);
            if (signal != null)
            {
                signal.SetAspect(SignalAspect.Speed40);
            }

            Debug.WriteLine($"[SIGNAL] Dodano semafor na ({right}, {y}) South");
        }

        // Dolna prosta (West) - Speed40
        for (int x = right - 5; x > left; x -= 10)
        {
            _signalController.AddSignal(
                new MapPosition(x, bottom),
                TrackConnections.West,
                new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop }
            );

            // ✅ Ustaw początkowy aspekt na Speed40
            var signal = _signalController.GetSignalAt(new MapPosition(x, bottom), TrackConnections.West);
            if (signal != null)
            {
                signal.SetAspect(SignalAspect.Speed40);
            }

            Debug.WriteLine($"[SIGNAL] Dodano semafor na ({x}, {bottom}) West");
        }

        // Lewa prosta (North) - Speed40
        for (int y = bottom - 5; y > top; y -= 10)
        {
            _signalController.AddSignal(
                new MapPosition(left, y),
                TrackConnections.North,
                new List<SignalAspect> { SignalAspect.Speed40, SignalAspect.Stop }
            );

            // ✅ Ustaw początkowy aspekt na Speed40
            var signal = _signalController.GetSignalAt(new MapPosition(left, y), TrackConnections.North);
            if (signal != null)
            {
                signal.SetAspect(SignalAspect.Speed40);
            }

            Debug.WriteLine($"[SIGNAL] Dodano semafor na ({left}, {y}) North");
        }

        // Semafor STOP na końcu (przed zakrętem na start)
        _signalController.AddSignal(
            new MapPosition(left, top + 1),
            TrackConnections.North,
            new List<SignalAspect> { SignalAspect.Stop }
        );
        Debug.WriteLine($"[SIGNAL] Dodano semafor STOP na ({left}, {top + 1}) North");

        Debug.WriteLine($"[SIGNAL] Łącznie dodano {_signalController.GetAllSignals().Count} semaforów");

        // ============================================================
        // 6. BLOKI
        // ============================================================
        if (_blockController != null)
        {
            _blockController.CreateBlocksFromSignals();
            Debug.WriteLine($"[BLOCK] Utworzono {_blockController.BlockCount} bloków");
        }
    }

    private void CreateTestTrain()
    {
        Debug.WriteLine("[TRAIN] Tworzę testowy pociąg...");

        var locomotiveParameters = new VehicleParameters(
            maxSpeed: 160.4f,
            acceleration: 0.8f,
            braking: 100.0f,
            mass: 80000f,
            length: 1.0f);

        var wagonParameters = new VehicleParameters(
            maxSpeed: 160.4f,
            acceleration: 0.8f,
            braking: 100.0f,
            mass: 40000f,
            length: 1.0f);

        // ✅ TWORZYMY LISTĘ POJAZDÓW
        var vehicles = new List<Vehicle>
    {
        new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters),
        new Wagon(wagonParameters),
        new Wagon(wagonParameters)
    };

        // ✅ TERAZ vehicles ISTNIEJE
        var train = new Train(
            spawnPosition: new Vector2(25.5f, 10.5f),
            initialDirection: TrackConnections.East,
            speed: 160.4f,
            vehicles: vehicles);  // ✅ OK!

        train.SetMap(_map);
        train.SetSignalController(_signalController);

        _trainManager.Add(train);

        Debug.WriteLine($"[TRAIN] Pociąg utworzony! ID: {train.Id}, Prędkość: {train.Speed}");
        Debug.WriteLine($"[TRAIN] Liczba pojazdów: {train.Composition.Vehicles.Count}");
    }
}