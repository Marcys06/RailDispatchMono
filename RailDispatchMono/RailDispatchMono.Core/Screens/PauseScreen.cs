using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Inputs;
using RailDispatchMono.Core.Localization;
using RailDispatchMono.Core.UI.Myra;
using System;

namespace RailDispatchMono.Core.Screens;

internal class PauseScreen : MenuScreen
{
    public event EventHandler? OnQuit;
    public event EventHandler? OnResume;
    public event EventHandler? OnSave;
    public event EventHandler? OnLoad;

    private bool _ignoreFirstCancel = true;
    private MyraPauseView? _myraView;

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

        if (ScreenManager.Game is RailDispatchMonoGame game)
        {
            _myraView = new MyraPauseView(
                ResumeFromMyra,
                SaveFromMyra,
                LoadFromMyra,
                QuitFromMyra,
                MenuEntries.Count > 2 && MenuEntries[2].Enabled);

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

    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        if (_ignoreFirstCancel)
        {
            _ignoreFirstCancel = false;
            return;
        }

        // Keep the legacy keyboard/controller contract, including ESC.
        // Myra Desktop handles the pointer interaction for the visible menu.
        base.HandleInput(gameTime, inputState);
    }

    private void ResumeFromMyra()
    {
        OnResume?.Invoke(this, EventArgs.Empty);
        ExitScreen();
    }

    private void SaveFromMyra()
        => OnSave?.Invoke(this, EventArgs.Empty);

    private void LoadFromMyra()
        => OnLoad?.Invoke(this, EventArgs.Empty);

    private void QuitFromMyra()
        => ShowQuitConfirmation();

    private void ResumeGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        => ResumeFromMyra();

    private void SaveGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        => SaveFromMyra();

    private void LoadGameEntrySelected(object? sender, PlayerIndexEventArgs e)
        => LoadFromMyra();

    private void QuitGameMenuEntrySelected(object? sender, PlayerIndexEventArgs e)
        => ShowQuitConfirmation();

    private void ShowQuitConfirmation()
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
        => ResumeFromMyra();

    public override void Draw(GameTime gameTime)
    {
        // Myra owns the visible pause menu. The shared desktop is rendered by
        // RailDispatchMonoGame after the ScreenManager screen stack.
    }
}
