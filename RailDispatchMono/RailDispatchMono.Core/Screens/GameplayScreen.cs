using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Game.Simulation;
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
            _map,
            _depotController);
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

    public void LoadSavedGame()
    {
        LoadMap();
    }
