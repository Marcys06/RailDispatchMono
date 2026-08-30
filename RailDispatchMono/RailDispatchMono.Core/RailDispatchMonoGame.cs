using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        _graphics.SynchronizeWithVerticalRetrace = true;

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
    }

    protected override void Initialize()
    {
        _screenManager = new ScreenManager(this);
        _gameplay = new GameplayScreen(GraphicsDevice, _screenManager);  // <- przekaż!
        _screenManager.AddScreen(_gameplay, null);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        // Zamień dotychczasowe _gameplay.LoadContent(); na:
        _gameplay.LoadContent(Content);
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
