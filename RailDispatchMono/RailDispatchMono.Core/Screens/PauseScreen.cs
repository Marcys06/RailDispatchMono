using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Localization;
using System;

namespace RailDispatchMono.Core.Screens
{
    internal class PauseScreen : MenuScreen
    {
        // ✅ ZDARZENIA DLA GAMEPLAYSCREEN
        public event EventHandler? OnQuit;
        public event EventHandler? OnResume;

        public PauseScreen() : base(Resources.Paused)
        {
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            MenuEntry resumeGameMenuEntry = new MenuEntry(Resources.Resume);
            MenuEntry quitGameMenuEntry = new MenuEntry(Resources.Quit);

            resumeGameMenuEntry.Selected += ResumeGameEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);

            DebugManager.Log($"[PAUSE] Konstruktor - Liczba entries: {MenuEntries.Count}");
            DebugManager.Log($"[PAUSE] Entry 1: '{resumeGameMenuEntry.Text}'");
            DebugManager.Log($"[PAUSE] Entry 2: '{quitGameMenuEntry.Text}'");
        }

        // ✅ DODAJ TĘ METODĘ!
        public override void Draw(GameTime gameTime)
        {
            DebugManager.Log($"[PAUSE] Draw() - Wywołuję base.Draw()");
            base.Draw(gameTime);
            DebugManager.Log($"[PAUSE] Draw() - Po base.Draw()");
        }

        private void ResumeGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        {
            OnResume?.Invoke(this, EventArgs.Empty);
            ExitScreen();
        }

        private void QuitGameMenuEntrySelected(object? sender, PlayerIndexEventArgs e)
        {
            string message = Resources.QuitQuestion;

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);
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
            // Nic nie rób - wróć do menu pauzy
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
    }
}