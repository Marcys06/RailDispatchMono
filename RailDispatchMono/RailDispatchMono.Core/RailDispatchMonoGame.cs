using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Screens;
using System;

namespace RailDispatchMono.Core;

public sealed class RailDispatchMonoGame : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
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
        _gameplay = new GameplayScreen(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _gameplay.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        GamePadState gamePad =
            GamePad.GetState(PlayerIndex.One);

        if (gamePad.Buttons.Back == ButtonState.Pressed)
        {
            Exit();
            return;
        }

        _gameplay.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _gameplay.Draw(gameTime);

        base.Draw(gameTime);
    }
}
