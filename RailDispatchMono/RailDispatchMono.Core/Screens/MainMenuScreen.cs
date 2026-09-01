using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Save;
using RailDispatchMono.Core.Localization;
using RailDispatchMono.Core.UI.Myra;
using System;

namespace RailDispatchMono.Core.Screens;

internal sealed class MainMenuScreen : MenuScreen
{
    private readonly Action<string> _startGame;
    private MyraMainMenuView? _myraView;

    public MainMenuScreen(Action<string> startGame) : base("RAIL DISPATCHER")
    {
        _startGame = startGame;

        // Preserve the existing MenuEntry contract while migrating the visual
        // and mouse/keyboard surface to the shared Myra desktop.
        var newGame = new MenuEntry("NOWA GRA");
        var loadGame = new MenuEntry("WCZYTAJ GRĘ");
        var settings = new MenuEntry(Resources.Settings);
        var about = new MenuEntry(Resources.About);
        var quit = new MenuEntry(Resources.Quit);

        newGame.Selected += (_, _) => StartNewGame();
        loadGame.Selected += (_, _) => OpenLoadMenu();
        settings.Selected += (_, _) => ScreenManager.AddScreen(new SettingsScreen(), ControllingPlayer);
        about.Selected += (_, _) => ScreenManager.AddScreen(new AboutScreen(), ControllingPlayer);
        quit.Selected += (_, _) => ScreenManager.Game.Exit();

        MenuEntries.Add(newGame);
        MenuEntries.Add(loadGame);
        MenuEntries.Add(settings);
        MenuEntries.Add(about);
        MenuEntries.Add(quit);
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager.Game is RailDispatchMonoGame game)
        {
            _myraView = new MyraMainMenuView(
                StartNewGame,
                OpenLoadMenu,
                () => ScreenManager.AddScreen(new SettingsScreen(), ControllingPlayer),
                () => ScreenManager.AddScreen(new AboutScreen(), ControllingPlayer),
                () => ScreenManager.Game.Exit());

            game.MyraUI.SetRoot(_myraView.Root);
        }
    }

    public override void UnloadContent()
    {
        if (ScreenManager.Game is RailDispatchMonoGame game)
            game.MyraUI.Clear();

        _myraView = null;
        base.UnloadContent();
    }

    public override void HandleInput(GameTime gameTime, RailDispatchMono.Core.Inputs.InputState inputState)
    {
        // Myra Desktop owns the main-menu pointer/keyboard interaction.
    }

    public override void Draw(GameTime gameTime)
    {
        // The shared Myra desktop is rendered by the game host after the
        // ScreenManager-owned screen stack.
    }

    private void StartNewGame()
    {
        try
        {
            string slot = SaveSlotService.CreateSlot();
            ExitScreen();
            _startGame("NEW:" + slot);
        }
        catch (Exception ex)
        {
            ScreenManager.AddScreen(new MessageBoxScreen("NIE MOŻNA UTWORZYĆ ZAPISU\n" + ex.Message), ControllingPlayer);
        }
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
                    SaveSlotService.Activate(slot.DirectoryPath);
                    ExitScreen();
                    startGame(slot.DirectoryPath);
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
