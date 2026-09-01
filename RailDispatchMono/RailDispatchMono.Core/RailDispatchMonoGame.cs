using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Screens;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.UI.Myra;
using System;

namespace RailDispatchMono.Core;

public sealed class RailDispatchMonoGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly MyraUIManager _myraUI;
    private ScreenManager _screenManager;
    private MainMenuScreen? _mainMenu;
    private GameplayScreen? _gameplay;

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
        // ScreenManager can load screens during its own initialization, before
        // Game.LoadContent() is reached. Myra must therefore receive the Game
        // instance before the first Myra widget is constructed.
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
        string slotDirectory = request.StartsWith("NEW:", StringComparison.Ordinal)
            ? request[4..]
            : request;

        SaveSlotService.Activate(slotDirectory);
        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager, loadExisting);
        _screenManager.AddScreen(_gameplay, null);

        if (loadExisting)
            _gameplay.LoadSavedGame();

        if (_mainMenu != null)
        {
            _screenManager.RemoveScreen(_mainMenu);
            _mainMenu = null;
        }
    }

    protected override void LoadContent()
    {
        // Initialize() establishes Myra before ScreenManager can construct any
        // Myra widgets. Initialize() is idempotent, so this remains safe for
        // the normal MonoGame content lifecycle.
        _myraUI.Initialize(this);
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        _myraUI.Render();
    }
}
