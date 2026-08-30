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
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;
        _graphics.SynchronizeWithVerticalRetrace = true;
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);

        DebugManager.LogInfo("Graphics configuration: 1280x720, VSync=true, 60 FPS fixed timestep.");
        DebugManager.LogInfo($"Debug log file: {DebugManager.LogFilePath}");
    }

    protected override void Initialize()
    {
        DebugManager.LogInfo("Game Initialize started.");

        _screenManager = new ScreenManager(this);
        _screenManager.TraceEnabled = true;
        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager);

        DebugManager.LogInfo("ScreenManager created; screen tracing enabled.");
        _screenManager.AddScreen(_gameplay, null);
        DebugManager.LogInfo("GameplayScreen added to ScreenManager.");

        base.Initialize();
        DebugManager.LogSuccess("Game Initialize completed.");
    }

    protected override void LoadContent()
    {
        DebugManager.LogInfo("Game LoadContent started.");
        _gameplay.LoadContent(Content);
        DebugManager.LogSuccess("Game LoadContent completed.");
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
