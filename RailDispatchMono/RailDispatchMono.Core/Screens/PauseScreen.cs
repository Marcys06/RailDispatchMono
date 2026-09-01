using Microsoft.Xna.Framework;
using RailDispatchMono.Core.Game.Debug;
using RailDispatchMono.Core.Inputs;
using RailDispatchMono.Core.UI.Myra;
using System;
using RailDispatchMono.Core.Localization;

namespace RailDispatchMono.Core.Screens;

internal sealed class PauseScreen : GameScreen
{
    public event EventHandler? OnQuit;
    public event EventHandler? OnResume;
    public event EventHandler? OnSave;
    public event EventHandler? OnLoad;

    private enum PauseCommand
    {
        None,
        Resume,
        Save,
        Load
    }

    private readonly bool _canLoad;
    private MyraPauseView? _myraView;
    private PauseCommand _pendingCommand;

    public PauseScreen(bool canLoad = true)
    {
        IsPopup = true;
        TransitionOnTime = TimeSpan.Zero;
        TransitionOffTime = TimeSpan.Zero;
        _canLoad = canLoad;
    }

    public override void LoadContent()
    {
        base.LoadContent();

        if (ScreenManager.Game is RailDispatchMonoGame game)
        {
            _myraView = new MyraPauseView(
                RequestResume,
                RequestSave,
                RequestLoad,
                QuitFromMyra,
                _canLoad);

            game.MyraUI.SetRoot(_myraView.Root);
        }
    }

    public override void UnloadContent()
    {
        if (ScreenManager.Game is RailDispatchMonoGame game)
            game.MyraUI.Clear();

        _myraView = null;
        _pendingCommand = PauseCommand.None;
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        // Myra raises Button.Click from Desktop.Render(). The callback only records
        // the requested operation. Execute it here, during the normal ScreenManager
        // update pass, never while Myra is rendering its widget tree.
        PauseCommand command = _pendingCommand;
        _pendingCommand = PauseCommand.None;

        switch (command)
        {
            case PauseCommand.Resume:
                DebugManager.Log("[PAUSE] Resume command consumed by PauseScreen.Update()");
                OnResume?.Invoke(this, EventArgs.Empty);
                break;

            case PauseCommand.Save:
                DebugManager.Log("[PAUSE] Save command consumed by PauseScreen.Update()");
                OnSave?.Invoke(this, EventArgs.Empty);
                _myraView?.SetLoadEnabled(true);
                break;

            case PauseCommand.Load:
                DebugManager.Log("[PAUSE] Load command consumed by PauseScreen.Update()");
                OnLoad?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        if (inputState.IsMenuCancel(ControllingPlayer, out _))
            RequestResume();
    }

    private void RequestResume()
        => _pendingCommand = PauseCommand.Resume;

    private void RequestSave()
        => _pendingCommand = PauseCommand.Save;

    private void RequestLoad()
        => _pendingCommand = PauseCommand.Load;

    private void QuitFromMyra()
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

    public override void Draw(GameTime gameTime)
    {
        // Myra owns the complete visible pause UI. No legacy MenuEntry is drawn here.
    }
}
