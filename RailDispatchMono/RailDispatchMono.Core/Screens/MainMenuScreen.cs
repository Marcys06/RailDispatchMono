using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Localization;
using System;

namespace RailDispatchMono.Core.Screens;

/// <summary>Main entry point for the game. Save selection happens before gameplay is created.</summary>
internal sealed class MainMenuScreen : MenuScreen
{
    private readonly Action<string> _startGame;
    private readonly MenuEntry _newGame;
    private readonly MenuEntry _loadGame;
    private readonly MenuEntry _settings;
    private readonly MenuEntry _about;
    private readonly MenuEntry _quit;

    public MainMenuScreen(Action<string> startGame) : base("RAIL DISPATCHER")
    {
        _startGame = startGame;
        _newGame = new MenuEntry("NOWA GRA");
        _loadGame = new MenuEntry("WCZYTAJ GRĘ");
        _settings = new MenuEntry(Resources.Settings);
        _about = new MenuEntry(Resources.About);
        _quit = new MenuEntry(Resources.Quit);

        _newGame.Selected += (_, _) => StartNewGame();
        _loadGame.Selected += (_, _) => OpenLoadMenu();
        _settings.Selected += (_, _) => ScreenManager.AddScreen(new SettingsScreen(), ControllingPlayer);
        _about.Selected += (_, _) => ScreenManager.AddScreen(new AboutScreen(), ControllingPlayer);
        _quit.Selected += (_, _) => ScreenManager.Game.Exit();

        MenuEntries.Add(_newGame);
        MenuEntries.Add(_loadGame);
        MenuEntries.Add(_settings);
        MenuEntries.Add(_about);
        MenuEntries.Add(_quit);
    }

    private void StartNewGame()
    {
        string slot = SaveSlotService.CreateSlot();
        ExitScreen();
        _startGame(slot);
    }

    private void OpenLoadMenu()
    {
        var slots = SaveSlotService.GetSlots();
        if (slots.Count == 0)
        {
            ScreenManager.AddScreen(new MessageBoxScreen("BRAK ZAPISANYCH GIER."), ControllingPlayer);
            return;
        }

        ScreenManager.AddScreen(new LoadGameScreen(slots, _startGame), ControllingPlayer);
    }
}

internal sealed class LoadGameScreen : MenuScreen
{
    public LoadGameScreen(System.Collections.Generic.IReadOnlyList<SaveSlotMetadata> slots, Action<string> startGame)
        : base("WCZYTAJ GRĘ")
    {
        foreach (SaveSlotMetadata slot in slots)
        {
            var entry = new MenuEntry(slot.Name);
            entry.Selected += (_, _) =>
            {
                try
                {
                    string directory = System.IO.Path.Combine(SaveSlotContext.RootDirectory, slot.Name.Replace(':', '-'));
                    if (!System.IO.Directory.Exists(directory))
                    {
                        foreach (string candidate in System.IO.Directory.GetDirectories(SaveSlotContext.RootDirectory))
                        {
                            if (System.IO.File.Exists(System.IO.Path.Combine(candidate, "metadata.json")))
                            {
                                var text = System.IO.File.ReadAllText(System.IO.Path.Combine(candidate, "metadata.json"));
                                if (text.Contains("\"Name\": \"" + slot.Name + "\"", StringComparison.OrdinalIgnoreCase))
                                {
                                    directory = candidate;
                                    break;
                                }
                            }
                        }
                    }
                    SaveSlotService.Activate(directory);
                    ExitScreen();
                    startGame(directory);
                }
                catch (Exception ex)
                {
                    ScreenManager.AddScreen(new MessageBoxScreen("NIE MOŻNA WCZYTAĆ GRY\n" + ex.Message), ControllingPlayer);
                }
            };
            MenuEntries.Add(entry);
        }
        var back = new MenuEntry(Resources.Back);
        back.Selected += OnCancel;
        MenuEntries.Add(back);
    }
}
