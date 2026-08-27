using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
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

    private MouseState _previousMouse;
    private int _previousScrollWheelValue;

    public GameplayScreen(
        GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        _spriteBatch =
            new SpriteBatch(
                graphicsDevice);

        _map =
            new GameMap(
                100,
                100);

        _builder =
            new TrackBuilder(
                _map);

        _camera =
            new Camera
            {
                Position = Vector2.Zero,
                Zoom = 32f
            };

        _renderer =
            new TrackRenderer(
                _map);

        _trainManager =
            new TrainManager();

        _trainRenderer =
            new TrainRenderer();

        CreateTestTrain();
    }

    public void LoadContent()
    {
        _renderer.LoadContent(
            _graphicsDevice);

        _trainRenderer.LoadContent(
            _graphicsDevice);
    }

    public void Update(
        GameTime gameTime)
    {
        var mouse =
            Mouse.GetState();

        var keyboard =
            Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.D1))
        {
            _builder.Mode =
                TrackBuildMode.Straight;
        }

        if (keyboard.IsKeyDown(Keys.D2))
        {
            _builder.Mode =
                TrackBuildMode.Curve;
        }

        if (keyboard.IsKeyDown(Keys.H))
        {
            _builder.StraightHorizontal =
                true;
        }

        if (keyboard.IsKeyDown(Keys.V))
        {
            _builder.StraightHorizontal =
                false;
        }

        if (keyboard.IsKeyDown(Keys.R))
        {
            _builder.Curve =
                _builder.Curve switch
                {
                    CurveDirection.NorthEast =>
                        CurveDirection.EastSouth,

                    CurveDirection.EastSouth =>
                        CurveDirection.SouthWest,

                    CurveDirection.SouthWest =>
                        CurveDirection.WestNorth,

                    CurveDirection.WestNorth =>
                        CurveDirection.NorthEast,

                    _ =>
                        CurveDirection.NorthEast
                };
        }

        if (mouse.LeftButton ==
            ButtonState.Pressed)
        {
            var screenPosition =
                new Vector2(
                    mouse.X,
                    mouse.Y);

            var mapPosition =
                _camera.ScreenToMap(
                    screenPosition);

            if (_builder.Mode ==
                TrackBuildMode.Straight)
            {
                _builder.BuildStraight(
                    mapPosition,
                    _builder.StraightHorizontal);
            }
            else
            {
                _builder.BuildCurve(
                    mapPosition,
                    _builder.Curve);
            }
        }

        if (mouse.RightButton ==
            ButtonState.Pressed)
        {
            var screenPosition =
                new Vector2(
                    mouse.X,
                    mouse.Y);

            var mapPosition =
                _camera.ScreenToMap(
                    screenPosition);

            _builder.Remove(
                mapPosition);
        }

        if (mouse.MiddleButton ==
            ButtonState.Pressed &&
            _previousMouse.MiddleButton ==
            ButtonState.Pressed)
        {
            var delta =
                new Vector2(
                    mouse.X -
                    _previousMouse.X,
                    mouse.Y -
                    _previousMouse.Y);

            if (_camera.Zoom > 0f)
            {
                _camera.Move(
                    -delta /
                    _camera.Zoom);
            }
        }

        var currentScroll =
            mouse.ScrollWheelValue;

        if (currentScroll !=
            _previousScrollWheelValue)
        {
            var delta =
                currentScroll >
                _previousScrollWheelValue
                    ? 2f
                    : -2f;

            _camera.ZoomAt(
                new Vector2(
                    mouse.X,
                    mouse.Y),
                delta);
        }

        _previousScrollWheelValue =
            currentScroll;

        _previousMouse =
            mouse;
    }

    public void Draw(
        GameTime gameTime)
    {
        var mouse =
            Mouse.GetState();

        var mouseScreenPosition =
            new Vector2(
                mouse.X,
                mouse.Y);

        var previewPosition =
            _camera.ScreenToMap(
                mouseScreenPosition);

        _spriteBatch.Begin(
            transformMatrix:
                _camera.Transform);

        _renderer.Draw(
            _spriteBatch,
            _camera);

        _trainRenderer.Draw(
            _spriteBatch,
            _trainManager);

        _renderer.DrawPreview(
            _spriteBatch,
            previewPosition,
            _builder.Mode,
            _builder.StraightHorizontal,
            _builder.Curve);

        _spriteBatch.End();
    }

    private void CreateTestTrain()
    {
        var locomotiveParameters =
            new VehicleParameters(
                maxSpeed: 120f,
                acceleration: 0.8f,
                braking: 1.0f,
                mass: 80000f,
                length: 1.0f);

        var wagonParameters =
            new VehicleParameters(
                maxSpeed: 120f,
                acceleration: 0f,
                braking: 1.0f,
                mass: 40000f,
                length: 1.0f);

        var train =
            new Train();

        train.Composition.AddVehicle(
            new Locomotive(
                LocomotiveType.ElectricDC,
                locomotiveParameters));

        train.Composition.AddVehicle(
            new Wagon(
                wagonParameters));

        train.Composition.AddVehicle(
            new Wagon(
                wagonParameters));

        _trainManager.Add(
            train);
    }
}
