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

        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            // The ESC that opened the pause menu is still the newest keyboard
            // transition when ScreenManager first gives this screen input.
            // Consume that transition here so opening pause does not instantly
            // close it again. A later, genuinely new ESC closes the menu.
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
            GraphicsDevice graphicsDevice = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);
            Texture2D overlay = new Texture2D(graphicsDevice, 1, 1);
            overlay.SetData(new[] { new Color(0, 0, 0, 170) });
            spriteBatch.Draw(overlay, new Rectangle(0, 0, ScreenManager.BaseScreenSize.X, ScreenManager.BaseScreenSize.Y), Color.White);
            overlay.Dispose();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}