using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Train;

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
        _trainDebugger = new TrainDebugger(1.0f); // Logowanie co 1 sekundę

        CreateTestTrack();
        CreateTestTrain();
    }

    public void LoadContent()
    {
        _renderer.LoadContent(_graphicsDevice);
        _trainRenderer.LoadContent(_graphicsDevice);
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var mouse = Mouse.GetState();
        var keyboard = Keyboard.GetState();

        // --- AKTUALIZACJA SYMULACJI I DEBUGA ---
        _trainManager.Update(deltaTime);
        _trainDebugger.Update(deltaTime, _trainManager);

        // --- OBSŁUGA KLAWIATURY ---
        HandleKeyboardInput(keyboard);

        // --- OBSŁUGA MYSZY (BUDOWANIE / KASOWANIE / KAMERA) ---
        HandleMouseInput(mouse);

        _previousScrollWheelValue = mouse.ScrollWheelValue;
        _previousMouse = mouse;
        _previousKeyboard = keyboard;
    }

    private void HandleKeyboardInput(KeyboardState keyboard)
    {
        // 1. Pasek budowania (Sloty 1 - 9)
        if (IsKeyPressed(keyboard, Keys.D1) || IsKeyPressed(keyboard, Keys.NumPad1)) _builder.Mode = TrackBuildMode.Straight;
        if (IsKeyPressed(keyboard, Keys.D2) || IsKeyPressed(keyboard, Keys.NumPad2)) _builder.Mode = TrackBuildMode.Curve;
        if (IsKeyPressed(keyboard, Keys.D3) || IsKeyPressed(keyboard, Keys.NumPad3)) _builder.Mode = TrackBuildMode.Junction;
        if (IsKeyPressed(keyboard, Keys.D4) || IsKeyPressed(keyboard, Keys.NumPad4)) _builder.Mode = TrackBuildMode.Signal;
        if (IsKeyPressed(keyboard, Keys.D5) || IsKeyPressed(keyboard, Keys.NumPad5)) _builder.Mode = TrackBuildMode.Station;
        if (IsKeyPressed(keyboard, Keys.D6) || IsKeyPressed(keyboard, Keys.NumPad6)) _builder.Mode = TrackBuildMode.Reserved6;
        if (IsKeyPressed(keyboard, Keys.D7) || IsKeyPressed(keyboard, Keys.NumPad7)) _builder.Mode = TrackBuildMode.Reserved7;
        if (IsKeyPressed(keyboard, Keys.D8) || IsKeyPressed(keyboard, Keys.NumPad8)) _builder.Mode = TrackBuildMode.Reserved8;
        if (IsKeyPressed(keyboard, Keys.D9) || IsKeyPressed(keyboard, Keys.NumPad9)) _builder.Mode = TrackBuildMode.Reserved9;
        if (IsKeyPressed(keyboard, Keys.D0) || IsKeyPressed(keyboard, Keys.NumPad0)) OnSlotReserved(10);

        // 2. Modyfikatory i orientacja torów
        if (keyboard.IsKeyDown(Keys.H)) _builder.StraightHorizontal = true;
        if (keyboard.IsKeyDown(Keys.V)) _builder.StraightHorizontal = false;

        if (IsKeyPressed(keyboard, Keys.R))
        {
            RotateSelectedBuilding();
        }

        if (IsKeyPressed(keyboard, Keys.Delete) || IsKeyPressed(keyboard, Keys.X)) OnActionReserved("DemolishMode");
        if (IsKeyPressed(keyboard, Keys.Escape)) OnActionReserved("CancelAction");
        if (IsKeyPressed(keyboard, Keys.C)) OnActionReserved("CopyTrack");
        if (IsKeyPressed(keyboard, Keys.Z)) OnActionReserved("Undo");
        if (IsKeyPressed(keyboard, Keys.Y)) OnActionReserved("Redo");

        // 3. Kontrola symulacji i czasu
        if (IsKeyPressed(keyboard, Keys.Space)) OnActionReserved("TogglePause");
        if (IsKeyPressed(keyboard, Keys.Tab)) OnActionReserved("CycleGameSpeed");
        if (IsKeyPressed(keyboard, Keys.OemPlus) || IsKeyPressed(keyboard, Keys.Add)) OnActionReserved("SpeedUp");
        if (IsKeyPressed(keyboard, Keys.OemMinus) || IsKeyPressed(keyboard, Keys.Subtract)) OnActionReserved("SpeedDown");

        // 4. Nakładki, widok i interfejs
        if (IsKeyPressed(keyboard, Keys.G)) OnActionReserved("ToggleGrid");
        if (IsKeyPressed(keyboard, Keys.M)) OnActionReserved("ToggleMinimap");
        if (IsKeyPressed(keyboard, Keys.T)) OnActionReserved("ToggleTimetable");
        if (IsKeyPressed(keyboard, Keys.L)) OnActionReserved("ToggleSignalsOverlay");
        if (IsKeyPressed(keyboard, Keys.I)) OnActionReserved("ToggleInfoPanel");
        if (IsKeyPressed(keyboard, Keys.E)) OnActionReserved("ToggleEconomy");

        // 5. Nawigacja kamerą
        if (IsKeyPressed(keyboard, Keys.Home)) OnActionReserved("ResetCamera");
        if (IsKeyPressed(keyboard, Keys.PageUp)) OnActionReserved("ZoomIn");
        if (IsKeyPressed(keyboard, Keys.PageDown)) OnActionReserved("ZoomOut");
        if (IsKeyPressed(keyboard, Keys.F)) OnActionReserved("FollowTrain");

        // 6. Narzędzia Deweloperskie i Debugger
        if (IsKeyPressed(keyboard, Keys.F1)) OnActionReserved("ShowHelp");
        if (IsKeyPressed(keyboard, Keys.F5)) OnActionReserved("QuickSave");
        if (IsKeyPressed(keyboard, Keys.F9)) OnActionReserved("QuickLoad");

        if (IsKeyPressed(keyboard, Keys.F11))
        {
            _trainDebugger.ForceLog(_trainManager);
        }

        if (IsKeyPressed(keyboard, Keys.F12))
        {
            _trainDebugger.IsEnabled = !_trainDebugger.IsEnabled;
        }

        if (IsKeyPressed(keyboard, Keys.OemTilde)) OnActionReserved("OpenConsole");
    }

    private void HandleMouseInput(MouseState mouse)
    {
        // Budowanie torów (LPM)
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            var screenPosition = new Vector2(mouse.X, mouse.Y);
            var mapPosition = _camera.ScreenToMap(screenPosition);
            _builder.BuildAt(mapPosition);
        }

        // Akcja dodatkowa / Kasowanie / Przełączanie zwrotnicy (PPM - kliknięcie pojedyncze)
        if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released)
        {
            var screenPosition = new Vector2(mouse.X, mouse.Y);
            var mapPosition = _camera.ScreenToMap(screenPosition);

            if (_map.TryGetTrack(mapPosition, out var track) && track is not null && track.IsJunction)
            {
                track.ToggleSwitch();
            }
            else
            {
                _builder.Remove(mapPosition);
            }
        }

        // Przesuwanie kamery (ŚPM)
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

        // Zoom kółkiem myszy
        var currentScroll = mouse.ScrollWheelValue;
        if (currentScroll != _previousScrollWheelValue)
        {
            var delta = currentScroll > _previousScrollWheelValue ? 2f : -2f;
            _camera.ZoomAt(new Vector2(mouse.X, mouse.Y), delta);
        }
    }

    private void RotateSelectedBuilding()
    {
        if (_builder.Mode == TrackBuildMode.Curve)
        {
            _builder.Curve = _builder.Curve switch
            {
                CurveDirection.NorthEast => CurveDirection.EastSouth,
                CurveDirection.EastSouth => CurveDirection.SouthWest,
                CurveDirection.SouthWest => CurveDirection.WestNorth,
                CurveDirection.WestNorth => CurveDirection.NorthEast,
                _ => CurveDirection.NorthEast
            };
        }
        else if (_builder.Mode == TrackBuildMode.Junction)
        {
            _builder.Junction = _builder.Junction switch
            {
                JunctionType.South_NorthEast => JunctionType.South_NorthWest,
                JunctionType.South_NorthWest => JunctionType.West_EastSouth,
                JunctionType.West_EastSouth => JunctionType.West_EastNorth,
                JunctionType.West_EastNorth => JunctionType.South_NorthEast,
                _ => JunctionType.South_NorthEast
            };
        }
    }

    private bool IsKeyPressed(KeyboardState currentKeyboard, Keys key)
    {
        return currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);
    }

    private void OnActionReserved(string actionName)
    {
        // Rezerwa na przyszłą funkcjonalność
    }

    private void OnSlotReserved(int slotIndex)
    {
        // Rezerwa pod kolejne sloty paska budowania
    }

    public void Draw(GameTime gameTime)
    {
        var mouse = Mouse.GetState();
        var mouseScreenPosition = new Vector2(mouse.X, mouse.Y);
        var previewPosition = _camera.ScreenToMap(mouseScreenPosition);

        _spriteBatch.Begin(transformMatrix: _camera.Transform);

        _renderer.Draw(_spriteBatch, _camera);
        _trainRenderer.Draw(_spriteBatch, _trainManager);

        _renderer.DrawPreview(
            _spriteBatch,
            previewPosition,
            _builder.Mode,
            _builder.StraightHorizontal,
            _builder.Curve,
            _builder.Junction);

        _spriteBatch.End();
    }

    private void CreateTestTrack()
    {
        const int left = 10;
        const int right = 30;
        const int top = 10;
        const int bottom = 29;

        // GÓRNY LEWY ZAKRĘT (East -> South)
        _builder.BuildCurve(new MapPosition(left, top), CurveDirection.EastSouth);

        // GÓRNA PROSTA (Horizontal)
        for (var x = left + 1; x < right; x++)
        {
            _builder.BuildStraight(new MapPosition(x, top), horizontal: true);
        }

        // GÓRNY PRAWY ZAKRĘT (South -> West)
        _builder.BuildCurve(new MapPosition(right, top), CurveDirection.SouthWest);

        // PRAWA PROSTA (Vertical)
        for (var y = top + 1; y < bottom; y++)
        {
            _builder.BuildStraight(new MapPosition(right, y), horizontal: false);
        }

        // DOLNY PRAWY ZAKRĘT (West -> North)
        _builder.BuildCurve(new MapPosition(right, bottom), CurveDirection.WestNorth);

        // DOLNA PROSTA (Horizontal)
        for (var x = right - 1; x > left; x--)
        {
            _builder.BuildStraight(new MapPosition(x, bottom), horizontal: true);
        }

        // DOLNY LEWY ZAKRĘT (North -> East)
        _builder.BuildCurve(new MapPosition(left, bottom), CurveDirection.NorthEast);

        // LEWA PROSTA (Vertical)
        for (var y = bottom - 1; y > top; y--)
        {
            _builder.BuildStraight(new MapPosition(left, y), horizontal: false);
        }
    }

    private void CreateTestTrain()
    {
        var locomotiveParameters = new VehicleParameters(
            maxSpeed: 20.4f,
            acceleration: 0.8f,
            braking: 1.0f,
            mass: 80000f,
            length: 1.0f);

        var wagonParameters = new VehicleParameters(
            maxSpeed: 20.4f,
            acceleration: 0.8f,
            braking: 1.0f,
            mass: 40000f,
            length: 1.0f);

        var train = new Train(
            new Vector2(25.5f, 10.5f),
            TrackConnections.East,
            speed: 20.4f);

        train.SetMap(_map);

        train.Composition.AddVehicle(new Locomotive(LocomotiveType.ElectricDC, locomotiveParameters));
        train.Composition.AddVehicle(new Wagon(wagonParameters));
        train.Composition.AddVehicle(new Wagon(wagonParameters));

        _trainManager.Add(train);
    }
}