using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Localization;
using System;

namespace RailDispatchMono.Core.Screens
{
    internal class PauseScreen : MenuScreen
    {
        // ✅ ZDARZENIA DLA GAMEPLAYSCREEN
        public event EventHandler? OnQuit;

        public PauseScreen() : base(Resources.Paused)
        {
            IsPopup = true;

            MenuEntry resumeGameMenuEntry = new MenuEntry(Resources.Resume);
            MenuEntry quitGameMenuEntry = new MenuEntry(Resources.Quit);

            // ✅ Wznów grę — użyj OnCancel (ESC lub kliknięcie)
            resumeGameMenuEntry.Selected += OnCancel;

            // ✅ Wyjście z gry
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }

        private void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            string message = Resources.QuitQuestion;

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);
            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }

        private void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            // ✅ Powiadom GameplayScreen o wyjściu
            OnQuit?.Invoke(this, EventArgs.Empty);

            // Wyjdź z gry
            ScreenManager.Game.Exit();
        }

        public void Cancel(PlayerIndex playerIndex)
        {
            OnCancel(playerIndex); // wywołuje chronioną metodę
        }

        // ✅ Nadpisanie OnCancel — ESC zamyka menu pauzy i wznawia grę
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            // ✅ To zamknie menu pauzy — GameplayScreen zareaguje na to
            ExitScreen();
        }
    }
}