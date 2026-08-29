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
        Debug.WriteLine("[TRACK] Tworzę testową trasę...");

        const int left = 10;
        const int right = 30;
        const int top = 10;
        const int bottom = 29;

        _builder.BuildCurve(new MapPosition(left, top), CurveDirection.EastSouth);

        for (var x = left + 1; x < right; x++)
        {
            if (x == 20 || x == 25)
            {
                Debug.WriteLine($"[TRACK] Dodaję zwrotnicę na ({x}, {top})");
                _builder.BuildJunctionFromType(new MapPosition(x, top), JunctionType.South_NorthEast);
            }
            else
            {
                _builder.BuildStraight(new MapPosition(x, top), horizontal: true);
            }
        }

        _builder.BuildCurve(new MapPosition(right, top), CurveDirection.SouthWest);

        for (var y = top + 1; y < bottom; y++)
        {
            _builder.BuildStraight(new MapPosition(right, y), horizontal: false);
        }

        _builder.BuildCurve(new MapPosition(right, bottom), CurveDirection.WestNorth);

        for (var x = right - 1; x > left; x--)
        {
            _builder.BuildStraight(new MapPosition(x, bottom), horizontal: true);
        }

        _builder.BuildCurve(new MapPosition(left, bottom), CurveDirection.NorthEast);

        for (var y = bottom - 1; y > top; y--)
        {
            _builder.BuildStraight(new MapPosition(left, y), horizontal: false);
        }

        Debug.WriteLine("[TRACK] Testowa trasa utworzona!");
    }

    private void CreateTestTrain()
    {
        Debug.WriteLine("[TRAIN] Tworzę testowy pociąg...");

        var locomotiveParameters = new VehicleParameters(
            maxSpeed: 8.4f,
            acceleration: 0.8f,
            braking: 1.0f,
            mass: 80000f,
            length: 1.0f);

        var wagonParameters = new VehicleParameters(
            maxSpeed: 8.4f,
            acceleration: 0.8f,
            braking: 1.0f,
            mass: 40000f,
            length: 1.0f);

        var train = new Train(
            new Vector2(25.5f, 10.5f),
            TrackConnections.East,
            speed: 8.4f);

        train.SetMap(_map);

        train.Composition.AddVehicle(new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters));
        train.Composition.AddVehicle(new Wagon(wagonParameters));
        train.Composition.AddVehicle(new Wagon(wagonParameters));

        _trainManager.Add(train);

        Debug.WriteLine($"[TRAIN] Pociąg utworzony! ID: {train.Id}, Prędkość: {train.Speed}");
        Debug.WriteLine($"[TRAIN] Liczba pojazdów: {train.Composition.Vehicles.Count}");
    }
}