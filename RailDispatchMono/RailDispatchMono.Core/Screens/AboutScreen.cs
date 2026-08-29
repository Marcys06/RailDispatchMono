using RailDispatchMono.Core.Localization;

namespace RailDispatchMono.Core.Screens
{
    /// <summary>
    /// Represents the "About" screen, providing information about the game and its technology.
    /// This screen displays credits and links to the MonoGame website.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="MenuScreen"/>, inheriting its menu management capabilities.
    /// </remarks>
    internal class AboutScreen : MenuScreen
    {
        private MenuEntry builtWithMonoGameMenuEntry;
        private MenuEntry monoGameWebsiteMenuEntry;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutScreen"/> class.
        /// </summary>
        public AboutScreen()
            : base(Resources.About)
        {
            // Create the static label entry. Disabled as it's a label
            builtWithMonoGameMenuEntry = new MenuEntry("#BuiltWithMonoGame", false);

            // Create the clickable link entry.
            monoGameWebsiteMenuEntry = new MenuEntry(Resources.MonoGameSite);

            // Create the "Back" button entry.
            MenuEntry back = new MenuEntry(Resources.Back);

            // Attach event handlers for menu entry selections.
            monoGameWebsiteMenuEntry.Selected += MonoGameWebsiteMenuSelected;
            back.Selected += OnCancel;

            // Add the menu entries to the screen.
            MenuEntries.Add(builtWithMonoGameMenuEntry);
            MenuEntries.Add(monoGameWebsiteMenuEntry);
            MenuEntries.Add(back);
        }

        /// <summary>
        /// Handles the selection event for the MonoGame website menu entry.
        /// </summary>

        // ZMIANA: object? zamiast object (usuwa ostrzeżenie CS8622)
        private void MonoGameWebsiteMenuSelected(object? sender, PlayerIndexEventArgs e)
        {
            LaunchDefaultBrowser("https://www.monogame.net/");
        }

        /// <summary>
        /// Launches the default web browser with the specified URL.
        /// </summary>
        private static void LaunchDefaultBrowser(string url)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}