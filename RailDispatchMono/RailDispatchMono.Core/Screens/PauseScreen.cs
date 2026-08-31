using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Save;
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

        public PauseScreen(bool canLoad = true) : base(Resources.Paused)
        {
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            MenuEntry resumeGameMenuEntry = new MenuEntry("WZNÓW GRĘ");
            MenuEntry saveGameMenuEntry = new MenuEntry("ZAPISZ GRĘ");
            MenuEntry loadGameMenuEntry = new MenuEntry("WCZYTAJ GRĘ") { Enabled = canLoad };
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

        private void ConfirmQuitMessageBoxCancelled(object? sender, PlayerIndexEventArgs e) { }

        protected override void OnCancel(PlayerIndex playerIndex)
        {
            OnResume?.Invoke(this, EventArgs.Empty);
            ExitScreen();
        }

        public void Cancel(PlayerIndex playerIndex) => OnCancel(playerIndex);
    }
}