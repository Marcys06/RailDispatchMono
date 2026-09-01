using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Railway;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Game.Rendering;
using RailDispatchMono.Core.Game.Simulation;
using RailDispatchMono.Core.Game.Train;
using RailDispatchMono.Core.Screens;
using RailDispatchMono.Core.Screens.UI;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.UI.Myra;
using System;
using System.Reflection;

namespace RailDispatchMono.Core;

public sealed class RailDispatchMonoGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly MyraUIManager _myraUI;
    private ScreenManager _screenManager;
    private MainMenuScreen? _mainMenu;
    private GameplayScreen? _gameplay;
    private MyraGameplayView? _gameplayView;
    private double _gameplayUiRefreshTimer;

    public static bool IsMobile => false;
    public static bool IsDesktop => true;

    public RailDispatchMonoGame()
    {
        DebugManager.LogInfo("RailDispatchMonoGame constructor started.");
        DebugManager.LogInfo($"Runtime: {Environment.OSVersion} | .NET: {Environment.Version}");
        _graphics = new GraphicsDeviceManager(this);
        _myraUI = new MyraUIManager();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowClientSizeChanged;
    }

    public MyraUIManager MyraUI => _myraUI;

    private void OnWindowClientSizeChanged(object? sender, EventArgs e)
        => DebugManager.LogInfo($"Window resized: {Window.ClientBounds.Width}x{Window.ClientBounds.Height}");

    protected override void Initialize()
    {
        _myraUI.Initialize(this);
        _screenManager = new ScreenManager(this) { TraceEnabled = false };
        Components.Add(_screenManager);
        _mainMenu = new MainMenuScreen(StartGameplay);
        _screenManager.AddScreen(_mainMenu, null);
        base.Initialize();
    }

    private void StartGameplay(string request)
    {
        bool loadExisting = !request.StartsWith("NEW:", StringComparison.Ordinal);
        string slotDirectory = request.StartsWith("NEW:", StringComparison.Ordinal) ? request[4..] : request;

        SaveSlotService.Activate(slotDirectory);

        // MainMenuScreen.UnloadContent() clears the shared Myra desktop.
        // Remove it before installing the gameplay root.
        if (_mainMenu != null)
        {
            _screenManager.RemoveScreen(_mainMenu);
            _mainMenu = null;
        }

        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager, loadExisting);
        _screenManager.AddScreen(_gameplay, null);

        if (loadExisting)
            _gameplay.LoadSavedGame();

        _gameplayView = new MyraGameplayView(
            speed => _myraUI.QueueAction(() => GameClock.Current?.SetSpeed(speed)),
            train => _myraUI.QueueAction(() => FocusTrain(train)),
            station => _myraUI.QueueAction(() => FocusStation(station)),
            mode => _myraUI.QueueAction(() => SetBuildMode(mode)),
            () => _myraUI.QueueAction(ToggleRouteEditMode));
        _myraUI.SetRoot(_gameplayView.Root);
        _gameplayUiRefreshTimer = 0d;
    }

    private void FocusTrain(Train train)
    {
        Camera? camera = GetGameplayField<Camera>("_camera");
        if (camera != null)
            camera.Position = train.Position;
    }

    private void FocusStation(Station station)
    {
        Camera? camera = GetGameplayField<Camera>("_camera");
        if (camera != null)
            camera.Position = new Vector2(
                station.Position.X + station.Width / 2f,
                station.Position.Y + station.Height / 2f);
    }

    private void SetBuildMode(TrackBuildMode mode)
    {
        TrackBuilder? builder = GetGameplayField<TrackBuilder>("_builder");
        if (builder != null)
            builder.Mode = mode;
    }

    private void ToggleRouteEditMode()
    {
        InputManager? input = GetGameplayField<InputManager>("_inputManager");
        if (input == null)
            return;

        FieldInfo? modeField = typeof(InputManager).GetField(
            "_wagonRouteEditMode",
            BindingFlags.Instance | BindingFlags.NonPublic);
        modeField?.SetValue(input, !(bool)(modeField.GetValue(input) ?? false));
    }

    private T? GetGameplayField<T>(string fieldName) where T : class
    {
        if (_gameplay == null)
            return null;

        FieldInfo? field = typeof(GameplayScreen).GetField(
            fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic);
        return field?.GetValue(_gameplay) as T;
    }

    protected override void LoadContent()
    {
        _myraUI.Initialize(this);
    }

    protected override void Update(GameTime gameTime)
    {
        _myraUI.Update(gameTime);

        if (_gameplayView != null && _myraUI.Desktop.Root == _gameplayView.Root)
        {
            _gameplayUiRefreshTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_gameplayUiRefreshTimer >= 0.5d)
            {
                _gameplayUiRefreshTimer = 0d;
                _gameplayView.Refresh();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        _myraUI.Render();
    }
}
