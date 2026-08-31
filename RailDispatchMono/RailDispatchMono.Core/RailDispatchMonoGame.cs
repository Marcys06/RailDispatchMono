using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Screens;
using RailDispatchMono.Core.ScreenManagers;
using System;
using System.Reflection;

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
        _screenManager = new ScreenManager(this) { TraceEnabled = true };
        Components.Add(_screenManager);
        _mainMenu = new MainMenuScreen(StartGameplay);
        _screenManager.AddScreen(_mainMenu, null);
        base.Initialize();
    }

    private void StartGameplay(string slotDirectory)
    {
        SaveSlotService.Activate(slotDirectory);
        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager);
        _screenManager.AddScreen(_gameplay, null);

        // GameplayScreen currently owns its legacy test bootstrap. For a menu-created
        // slot, immediately load the selected slot so New Game starts empty and Load
        // restores the persisted state. This bridge can be removed when the gameplay
        // bootstrap is moved into an explicit scenario factory.
        if (_mainMenu != null)
        {
            try
            {
                MethodInfo? load = typeof(GameplayScreen).GetMethod("LoadMap", BindingFlags.Instance | BindingFlags.NonPublic);
                load?.Invoke(_gameplay, null);
            }
            catch (Exception ex)
            {
                DebugManager.LogWarning("[STARTUP] Save load failed: " + ex.Message);
            }
            _screenManager.RemoveScreen(_mainMenu);
            _mainMenu = null;
        }
    }

    protected override void LoadContent()
    {
        // ScreenManager is a DrawableGameComponent and loads every registered screen.
        // GameplayScreen is therefore loaded when it is added after initialization.
    }

    protected override void Update(GameTime gameTime)
    {
        _screenManager.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _screenManager.Draw(gameTime);
        base.Draw(gameTime);
    }
}
