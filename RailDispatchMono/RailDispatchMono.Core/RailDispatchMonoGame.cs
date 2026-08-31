using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.ScreenManagers;
using RailDispatchMono.Core.Screens;
using System;

namespace RailDispatchMono.Core;

public sealed class RailDispatchMonoGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private ScreenManager _screenManager;
    private GameplayScreen _gameplay;

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
        DebugManager.LogInfo("Graphics configuration: 1600x900, VSync=true, user-resizable window, 60 FPS fixed timestep.");
    }

    private void OnWindowClientSizeChanged(object? sender, EventArgs e)
    {
        DebugManager.LogInfo($"Window resized: {Window.ClientBounds.Width}x{Window.ClientBounds.Height}");
    }

    protected override void Initialize()
    {
        _screenManager = new ScreenManager(this);
        _screenManager.TraceEnabled = true;
        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager);
        _screenManager.AddScreen(_gameplay, null);
        base.Initialize();
    }

    protected override void LoadContent() => _gameplay.LoadContent(Content);
    protected override void Update(GameTime gameTime){_screenManager.Update(gameTime);base.Update(gameTime);}
    protected override void Draw(GameTime gameTime){_screenManager.Draw(gameTime);base.Draw(gameTime);}
}
