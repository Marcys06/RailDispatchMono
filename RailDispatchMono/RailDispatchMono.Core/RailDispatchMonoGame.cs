using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Screens;
using RailDispatchMono.Core.ScreenManagers;
using System;

namespace RailDispatchMono.Core;

public sealed class RailDispatchMonoGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
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

    private void OnWindowClientSizeChanged(object? sender, EventArgs e)
        => DebugManager.LogInfo($"Window resized: {Window.ClientBounds.Width}x{Window.ClientBounds.Height}");

    protected override void Initialize()
    {
        _screenManager = new ScreenManager(this) { TraceEnabled = false };
        Components.Add(_screenManager);
        _mainMenu = new MainMenuScreen(StartGameplay);
        _screenManager.AddScreen(_mainMenu, null);
        base.Initialize();
    }

    private void StartGameplay(string slotDirectory)
    {
        bool loadExisting = !string.IsNullOrWhiteSpace(SaveSlotContext.ActiveSlotDirectory)
                             && string.Equals(
                                 System.IO.Path.GetFullPath(SaveSlotContext.ActiveSlotDirectory),
                                 System.IO.Path.GetFullPath(slotDirectory),
                                 StringComparison.OrdinalIgnoreCase);

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
        // ScreenManager is a DrawableGameComponent and owns screen content loading.
    }

    protected override void Update(GameTime gameTime)
    {
        // ScreenManager is registered in Components. Game invokes it automatically.
        // Do not call _screenManager.Update here: doing so updates every screen twice per frame.
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // ScreenManager is a DrawableGameComponent and is drawn by Game automatically.
        base.Draw(gameTime);
    }
}
