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

            MenuEntries.Add(CreateEntry("WZNÓW GRĘ", ResumeGameEntrySelected));
            MenuEntries.Add(CreateEntry("ZAPISZ GRĘ", SaveGameEntrySelected));
            MenuEntries.Add(CreateEntry("WCZYTAJ GRĘ", LoadGameEntrySelected, canLoad));
            MenuEntries.Add(CreateEntry(Resources.Quit, QuitGameMenuEntrySelected));
        }

        private static MenuEntry CreateEntry(
            string text,
            EventHandler<PlayerIndexEventArgs> handler,
            bool enabled = true)
        {
            MenuEntry entry = new MenuEntry(text, enabled);
            entry.Selected += handler;
            return entry;
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
            _overlayTexture?.Dispose();
            _overlayTexture = null;
            base.UnloadContent();
        }

        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            if (_ignoreFirstCancel)
            {
                _ignoreFirstCancel = false;
                return;
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

        public override void Draw(GameTime gameTime)
        {
            // ===== ZABEZPIECZENIA PRZED NULL =====
            if (ScreenManager == null)
                return;

            if (ScreenManager.GraphicsDevice == null)
                return;

            if (ScreenManager.SpriteBatch == null)
                return;

            if (ScreenManager.Font == null)
                return;
            // ====================================

            GraphicsDevice graphicsDevice = ScreenManager.GraphicsDevice;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            if (_overlayTexture == null)
            {
                _overlayTexture = new Texture2D(graphicsDevice, 1, 1);
                _overlayTexture.SetData(new[] { new Color(0, 0, 0, 170) });
            }

            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
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