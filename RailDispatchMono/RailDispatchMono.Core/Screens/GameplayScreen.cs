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

        _trainDebugger = new TrainDebugger(1.0f); // Log co 1 sekundę

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

        // Aktualizuj pociągi
        _trainManager.Update(deltaTime);

        // Debugger
        _trainDebugger.Update(deltaTime, _trainManager);

        // Klawisze debugowania
        if (keyboard.IsKeyDown(Keys.F12))
        {
            _trainDebugger.IsEnabled = !_trainDebugger.IsEnabled;
            System.Threading.Thread.Sleep(100);
        }
        if (keyboard.IsKeyDown(Keys.F11))
        {
            _trainDebugger.ForceLog(_trainManager);
            System.Threading.Thread.Sleep(100);
        }

        // Tryby budowania
        if (keyboard.IsKeyDown(Keys.D1))
        {
            _builder.Mode = TrackBuildMode.Straight;
        }

        if (keyboard.IsKeyDown(Keys.D2))
        {
            _builder.Mode = TrackBuildMode.Curve;
        }

        if (keyboard.IsKeyDown(Keys.H))
        {
            _builder.StraightHorizontal = true;
        }

        if (keyboard.IsKeyDown(Keys.V))
        {
            _builder.StraightHorizontal = false;
        }

        if (keyboard.IsKeyDown(Keys.R))
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

        // Budowanie torów
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            var screenPosition = new Vector2(mouse.X, mouse.Y);
            var mapPosition = _camera.ScreenToMap(screenPosition);

            if (_builder.Mode == TrackBuildMode.Straight)
            {
                _builder.BuildStraight(mapPosition, _builder.StraightHorizontal);
            }
            else
            {
                _builder.BuildCurve(mapPosition, _builder.Curve);
            }
        }

        // Usuwanie torów
        if (mouse.RightButton == ButtonState.Pressed)
        {
            var screenPosition = new Vector2(mouse.X, mouse.Y);
            var mapPosition = _camera.ScreenToMap(screenPosition);
            _builder.Remove(mapPosition);
        }

        // Przesuwanie kamery
        if (mouse.MiddleButton == ButtonState.Pressed &&
            _previousMouse.MiddleButton == ButtonState.Pressed)
        {
            var delta = new Vector2(
                mouse.X - _previousMouse.X,
                mouse.Y - _previousMouse.Y);

            if (_camera.Zoom > 0f)
            {
                _camera.Move(-delta / _camera.Zoom);
            }
        }

        // Zoom
        var currentScroll = mouse.ScrollWheelValue;

        if (currentScroll != _previousScrollWheelValue)
        {
            var delta = currentScroll > _previousScrollWheelValue ? 2f : -2f;
            _camera.ZoomAt(new Vector2(mouse.X, mouse.Y), delta);
        }

        _previousScrollWheelValue = currentScroll;
        _previousMouse = mouse;
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
            _builder.Curve);

        _spriteBatch.End();
    }

    private void CreateTestTrack()
    {
        const int left = 10;
        const int right = 30;
        const int top = 10;
        const int bottom = 29;

        // GÓRNY LEWY ZAKRĘT (East -> South)
        _builder.BuildCurve(
            new MapPosition(left, top),
            CurveDirection.EastSouth);

        // GÓRNA PROSTA (Horizontal)
        for (var x = left + 1; x < right; x++)
        {
            _builder.BuildStraight(
                new MapPosition(x, top),
                horizontal: true);
        }

        // GÓRNY PRAWY ZAKRĘT (South -> West)
        _builder.BuildCurve(
            new MapPosition(right, top),
            CurveDirection.SouthWest);

        // PRAWA PROSTA (Vertical)
        for (var y = top + 1; y < bottom; y++)
        {
            _builder.BuildStraight(
                new MapPosition(right, y),
                horizontal: false);
        }

        // DOLNY PRAWY ZAKRĘT (West -> North)
        _builder.BuildCurve(
            new MapPosition(right, bottom),
            CurveDirection.WestNorth);

        // DOLNA PROSTA (Horizontal)
        for (var x = right - 1; x > left; x--)
        {
            _builder.BuildStraight(
                new MapPosition(x, bottom),
                horizontal: true);
        }

        // DOLNY LEWY ZAKRĘT (North -> East)
        _builder.BuildCurve(
            new MapPosition(left, bottom),
            CurveDirection.NorthEast);

        // LEWA PROSTA (Vertical)
        for (var y = bottom - 1; y > top; y--)
        {
            _builder.BuildStraight(
                new MapPosition(left, y),
                horizontal: false);
        }
    }

    private void CreateTestTrain()
    {
        // ============================================================
        // PARAMETRY POCIĄGU
        // ============================================================

        var locomotiveParameters = new VehicleParameters(
            maxSpeed: 2.0f,
            acceleration: 0.8f,
            braking: 1.0f,
            mass: 80000f,
            length: 1.0f);

        var wagonParameters = new VehicleParameters(
            maxSpeed: 2.0f,
            acceleration: 0.8f,
            braking: 1.0f,
            mass: 40000f,
            length: 1.0f);

        // ============================================================
        // POCIĄG Z LOKOMOTYWĄ I WAGONAMI
        // ============================================================

        var train = new Train(
            new Vector2(25.5f, 10.5f),
            TrackConnections.East,
            speed: 1.5f);

        train.SetMap(_map);

        // LOKOMOTYWA
        train.Composition.AddVehicle(
            new Locomotive(
                LocomotiveType.ElectricDC,
                locomotiveParameters));

        // ✅ WAGONY - PRZYWRÓCONE
        train.Composition.AddVehicle(new Wagon(wagonParameters));
        train.Composition.AddVehicle(new Wagon(wagonParameters));

        // ✅ OPCJONALNIE - WIĘCEJ WAGONÓW
        // train.Composition.AddVehicle(new Wagon(wagonParameters));
        // train.Composition.AddVehicle(new Wagon(wagonParameters));

        _trainManager.Add(train);
    }
}