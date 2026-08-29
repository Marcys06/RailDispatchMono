using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Localization;
namespace RailDispatchMono.Core.Screens
{
    internal class PauseScreen : MenuScreen
    {
        public PauseScreen()
            : base(Resources.Paused)
        {
            MenuEntry resumeGameMenuEntry = new MenuEntry(Resources.Resume);
            MenuEntry quitGameMenuEntry = new MenuEntry(Resources.Quit);

            resumeGameMenuEntry.Selected += OnCancel;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }

        private void QuitGameMenuEntrySelected(
            object sender,
            PlayerIndexEventArgs e)
        {
            string message = Resources.QuitQuestion;

            MessageBoxScreen confirmQuitMessageBox =
                new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted +=
                ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(
                confirmQuitMessageBox,
                ControllingPlayer);
        }

        private void ConfirmQuitMessageBoxAccepted(
            object sender,
            PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }
    }
}