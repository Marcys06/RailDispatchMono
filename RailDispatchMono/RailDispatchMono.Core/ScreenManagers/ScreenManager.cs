using FrameworkGame = Microsoft.Xna.Framework.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using RailDispatchMono.Core.Inputs;
using RailDispatchMono.Core.Screens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RailDispatchMono.Core.ScreenManagers
{
    /// <summary>
    /// The ScreenManager is a component responsible for managing multiple <see cref="GameScreen"/> instances.
    /// It maintains a stack of screens, invokes their Update and Draw methods, and automatically routes input
    /// to the topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        // List of active screens and screens pending update.
        private readonly List<GameScreen> screens = new List<GameScreen>();
        private readonly List<GameScreen> screensToUpdate = new List<GameScreen>();

        // Manages player input.
        private readonly InputState inputState = new InputState();

        // Shared resources for drawing and content management.
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Texture2D blankTexture;

        private bool isInitialized;
        private bool traceEnabled;

        internal const int BASE_BUFFER_WIDTH = 800;
        internal const int BASE_BUFFER_HEIGHT = 480;

        private int backbufferWidth;
        /// <summary>Gets or sets the current backbuffer width.</summary>
        public int BackbufferWidth { get => backbufferWidth; set => backbufferWidth = value; }

        private int backbufferHeight;
        /// <summary>Gets or sets the current backbuffer height.</summary>
        public int BackbufferHeight { get => backbufferHeight; set => backbufferHeight = value; }

        private Vector2 baseScreenSize = new Vector2(BASE_BUFFER_WIDTH, BASE_BUFFER_HEIGHT);
        /// <summary>Gets or sets the base screen size used for scaling calculations.</summary>
        public Vector2 BaseScreenSize { get => baseScreenSize; set => baseScreenSize = value; }

        private Matrix globalTransformation;
        /// <summary>Gets or sets the global transformation matrix for scaling and positioning.</summary>
        public Matrix GlobalTransformation { get => globalTransformation; set => globalTransformation = value; }

        /// <summary>
        /// Provides access to a shared SpriteBatch instance for drawing operations.
        /// </summary>
        public SpriteBatch SpriteBatch => spriteBatch;

        /// <summary>
        /// Provides access to a shared SpriteFont instance for text rendering.
        /// </summary>
        public SpriteFont Font => font;

        /// <summary>
        /// Enables or disables screen tracing for debugging purposes.
        /// When enabled, the manager prints a list of active screens during updates.
        /// </summary>
        public bool TraceEnabled { get => traceEnabled; set => traceEnabled = value; }

        Rectangle safeArea = new Rectangle(0, 0, BASE_BUFFER_WIDTH, BASE_BUFFER_HEIGHT);
        /// <summary>
        /// Returns the portion of the screen where drawing is safely allowed.
        /// </summary>
        public Rectangle SafeArea
        {
            get
            {
                return safeArea;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenManager"/> class.
        /// </summary>
        /// <param name="game">The associated Game instance.</param>
        public ScreenManager(FrameworkGame game) : base(game)
        {
            TouchPanel.EnabledGestures = GestureType.None;
        }

        /// <summary>
        /// Initializes the ScreenManager and any required services.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            isInitialized = true;
        }

        /// <summary>
        /// Loads graphical content for the ScreenManager and all active screens.
        /// </summary>
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

        /// <summary>
        /// Unloads graphical content for all screens.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }

        /// <summary>
        /// Updates the active screens and processes input.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of the game's timing state.</param>
        public override void Update(GameTime gameTime)
        {
            inputState.Update(gameTime, BaseScreenSize);
            screensToUpdate.Clear();
            screensToUpdate.AddRange(screens);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            while (screensToUpdate.Count > 0)
            {
                GameScreen screen = screensToUpdate[^1];
                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

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

        /// <summary>
        /// Prints active screen names to the debug console for diagnostic purposes.
        /// </summary>
        private void TraceScreens()
        {
            var screenNames = screens.Select(screen => screen.GetType().Name).ToList();
            DebugManager.Log(string.Join(", ", screenNames));
        }

        /// <summary>
        /// Draws the active screens.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of the game's timing state.</param>
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

        /// <summary>
        /// Releases resources used by the <see cref="ScreenManager"/> object.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    // Dispose of managed resources.
                    spriteBatch?.Dispose();
                }
            }
            finally
            {
                // Call the base class's Dispose method to ensure proper cleanup.
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Adds a new screen to the ScreenManager.
        /// </summary>
        /// <param name="screen">The screen to add.</param>
        /// <param name="controllingPlayer">The controlling player, if applicable.</param>
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

        /// <summary>
        /// Removes a screen from the ScreenManager.
        /// </summary>
        /// <param name="screen">The screen to remove.</param>
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
                TouchPanel.EnabledGestures = screens[^1].EnabledGestures;
            }
        }

        /// <summary>
        /// Returns an array of all active screens managed by the ScreenManager.
        /// </summary>
        /// <returns>
        /// An array containing all current GameScreen instances.
        /// </returns>
        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }

        /// <summary>
        /// Draws a translucent black fullscreen sprite.
        /// </summary>
        /// <param name="alpha">The opacity level of the fade (0 = fully transparent, 1 = fully opaque).</param>
        public void FadeBackBufferToBlack(float alpha)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, null);

            spriteBatch.Draw(blankTexture,
                             new Rectangle(0, 0, backbufferWidth, backbufferHeight),
                             Color.Black * alpha);

            spriteBatch.End();
        }

        /// <summary>
        /// Scales the game presentation area to match the screen's aspect ratio.
        /// </summary>
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