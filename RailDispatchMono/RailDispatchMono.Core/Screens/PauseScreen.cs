using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Inputs;
using RailDispatchMono.Core.Localization;
using System;

namespace RailDispatchMono.Core.Screens
{
    internal class PauseScreen : MenuScreen
    {
        public event EventHandler? OnQuit;
        public event EventHandler? OnResume;
        public event EventHandler? OnSave;
        public event EventHandler? OnLoad;

        private bool _ignoreFirstCancel = true;
        private Texture2D? _overlayTexture;

        public PauseScreen(bool canLoad = true) : base(Resources.Paused)
        {
            IsPopup = true;
            TransitionOnTime = TimeSpan.Zero;
            TransitionOffTime = TimeSpan.Zero;

            MenuEntry resumeGameMenuEntry = new MenuEntry("WZNÓW GRĘ");
            MenuEntry saveGameMenuEntry = new MenuEntry("ZAPISZ GRĘ");
            MenuEntry loadGameMenuEntry = new MenuEntry("WCZYTAJ GRĘ")
            {
                Enabled = canLoad
            };
            MenuEntry quitGameMenuEntry = new MenuEntry(Resources.Quit);

            resumeGameMenuEntry.Selected += ResumeGameEntrySelected;
            saveGameMenuEntry.Selected += SaveGameEntrySelected;
            loadGameMenuEntry.Selected += LoadGameEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(saveGameMenuEntry);
            MenuEntries.Add(loadGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }

        public override void LoadContent()
        {
            base.LoadContent();

            if (ScreenManager != null && ScreenManager.GraphicsDevice != null)
            {
                _overlayTexture = new Texture2D(ScreenManager.GraphicsDevice, 1, 1);
                _overlayTexture.SetData(new[] { new Color(0, 0, 0, 170) });
            }
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
            _overlayTexture?.Dispose();
            _overlayTexture = null;
        }

        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            if (_ignoreFirstCancel)
            {
                PlayerIndex ignoredPlayer;
                if (inputState.IsMenuCancel(ControllingPlayer, out ignoredPlayer))
                {
                    _ignoreFirstCancel = false;
                    return;
                }
                _ignoreFirstCancel = false;
            }

            base.HandleInput(gameTime, inputState);
        }

        private void ResumeGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        {
            OnResume?.Invoke(this, EventArgs.Empty);
            ExitScreen();
        }

        private void SaveGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        {
            OnSave?.Invoke(this, EventArgs.Empty);
        }

        private void LoadGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        {
            OnLoad?.Invoke(this, EventArgs.Empty);
        }

        private void QuitGameMenuEntrySelected(object? sender, PlayerIndexEventArgs e)
        {
            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(Resources.QuitQuestion);
            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;
            confirmQuitMessageBox.Cancelled += ConfirmQuitMessageBoxCancelled;
            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }

        private void ConfirmQuitMessageBoxAccepted(object? sender, PlayerIndexEventArgs e)
        {
            OnQuit?.Invoke(this, EventArgs.Empty);
            ScreenManager.Game.Exit();
        }

        private void ConfirmQuitMessageBoxCancelled(object? sender, PlayerIndexEventArgs e)
        {
        }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            OnResume?.Invoke(this, EventArgs.Empty);
            ExitScreen();
        }

        public void Cancel(PlayerIndex playerIndex)
        {
            OnCancel(playerIndex);
        }

        public override void Draw(GameTime gameTime)
        {
            if (ScreenManager == null)
                return;

            GraphicsDevice graphicsDevice = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            if (graphicsDevice == null || spriteBatch == null)
                return;

            if (_overlayTexture == null)
            {
                _overlayTexture = new Texture2D(graphicsDevice, 1, 1);
                _overlayTexture.SetData(new[] { new Color(0, 0, 0, 170) });
            }

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                null,
                null,
                null,
                null,
                null,
                ScreenManager.GlobalTransformation);

            spriteBatch.Draw(
                _overlayTexture,
                new Rectangle(
                    0,
                    0,
                    (int)ScreenManager.BaseScreenSize.X,
                    (int)ScreenManager.BaseScreenSize.Y),
                Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}