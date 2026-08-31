using FrameworkGame = Microsoft.Xna.Framework.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using RailDispatchMono.Core.Inputs;
using RailDispatchMono.Core.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RailDispatchMono.Core.ScreenManagers
{
    public class ScreenManager : DrawableGameComponent
    {
        private readonly List<GameScreen> screens = new List<GameScreen>();
        private readonly List<GameScreen> screensToUpdate = new List<GameScreen>();

        private readonly InputState inputState = new InputState();

        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D blankTexture;

        private bool isInitialized;
        private bool traceEnabled;

        internal const int BASE_BUFFER_WIDTH = 800;
        internal const int BASE_BUFFER_HEIGHT = 480;

        private int backbufferWidth;
        public int BackbufferWidth { get => backbufferWidth; set => backbufferWidth = value; }

        private int backbufferHeight;
        public int BackbufferHeight { get => backbufferHeight; set => backbufferHeight = value; }

        private Vector2 baseScreenSize = new Vector2(BASE_BUFFER_WIDTH, BASE_BUFFER_HEIGHT);
        public Vector2 BaseScreenSize { get => baseScreenSize; set => baseScreenSize = value; }

        private Matrix globalTransformation;
        public Matrix GlobalTransformation { get => globalTransformation; set => globalTransformation = value; }

        public SpriteBatch SpriteBatch => spriteBatch;
        public SpriteFont Font => font;
        public InputState InputState => inputState;
        public bool TraceEnabled { get => traceEnabled; set => traceEnabled = value; }

        Rectangle safeArea = new Rectangle(0, 0, BASE_BUFFER_WIDTH, BASE_BUFFER_HEIGHT);
        public Rectangle SafeArea => safeArea;

        public ScreenManager(FrameworkGame game) : base(game)
        {
            TouchPanel.EnabledGestures = GestureType.None;
        }

        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        protected override void LoadContent()
        {
            ContentManager content = Game.Content;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = content.Load<SpriteFont>("Fonts/Hud");
            blankTexture = content.Load<Texture2D>("Sprites/blank");

            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }

        protected override void UnloadContent()
        {
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }

        public override void Update(GameTime gameTime)
        {
            inputState.Update(gameTime, BaseScreenSize);
            screensToUpdate.Clear();
            screensToUpdate.AddRange(screens);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            while (screensToUpdate.Count > 0)
            {
                int lastIndex = screensToUpdate.Count - 1;
                GameScreen screen = screensToUpdate[lastIndex];
                screensToUpdate.RemoveAt(lastIndex);

                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active)
                {
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(gameTime, inputState);
                        otherScreenHasFocus = true;
                    }

                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }

            if (traceEnabled)
                TraceScreens();
        }

        private void TraceScreens()
        {
            var screenNames = screens.Select(screen => screen.GetType().Name).ToList();
            DebugManager.Log(string.Join(", ", screenNames));
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (var screen in screens)
            {
                if (screen.ScreenState != ScreenState.Hidden)
                {
                    screen.Draw(gameTime);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    spriteBatch?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public void AddScreen(GameScreen screen, PlayerIndex? controllingPlayer)
        {
            screen.ControllingPlayer = controllingPlayer;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            if (isInitialized)
            {
                screen.LoadContent();
            }

            screens.Add(screen);
            TouchPanel.EnabledGestures = screen.EnabledGestures;
        }

        public void RemoveScreen(GameScreen screen)
        {
            if (isInitialized)
            {
                screen.UnloadContent();
            }

            screens.Remove(screen);
            screensToUpdate.Remove(screen);

            if (screens.Count > 0)
            {
                int lastIndex = screens.Count - 1;
                TouchPanel.EnabledGestures = screens[lastIndex].EnabledGestures;
            }
        }

        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }

        public void FadeBackBufferToBlack(float alpha)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, backbufferWidth, backbufferHeight),
                             Color.Black * alpha);

            spriteBatch.End();
        }

        public void ScalePresentationArea()
        {
            if (GraphicsDevice == null || baseScreenSize.X <= 0 || baseScreenSize.Y <= 0)
            {
                throw new InvalidOperationException("Invalid graphics configuration");
            }

            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;

            if (backbufferHeight == 0 || baseScreenSize.Y == 0)
            {
                return;
            }

            float baseAspectRatio = baseScreenSize.X / baseScreenSize.Y;
            float screenAspectRatio = backbufferWidth / (float)backbufferHeight;

            float scalingFactor;
            float horizontalOffset = 0;
            float verticalOffset = 0;

            if (screenAspectRatio > baseAspectRatio)
            {
                scalingFactor = backbufferHeight / baseScreenSize.Y;
                horizontalOffset = (backbufferWidth - baseScreenSize.X * scalingFactor) / 2;
            }
            else
            {
                scalingFactor = backbufferWidth / baseScreenSize.X;
                verticalOffset = (backbufferHeight - baseScreenSize.Y * scalingFactor) / 2;
            }

            globalTransformation = Matrix.CreateScale(scalingFactor) *
                                   Matrix.CreateTranslation(horizontalOffset, verticalOffset, 0);

            inputState.UpdateInputTransformation(Matrix.Invert(globalTransformation));
        }
    }
}