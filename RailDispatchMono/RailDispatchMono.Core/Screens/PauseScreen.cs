using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Inputs;
using RailDispatchMono.Core.Localization;
using RailDispatchMono.Core.UI.Myra;
using System;

namespace RailDispatchMono.Core.Screens;

internal sealed class PauseScreen : GameScreen
{
    public event EventHandler? OnQuit;
    public event EventHandler? OnResume;
    public event EventHandler? OnSave;
    public event EventHandler? OnLoad;

    private readonly bool _canLoad;
    private MyraPauseView? _myraView;

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
                ResumeFromMyra,
                SaveFromMyra,
                LoadFromMyra,
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
        base.UnloadContent();
    }

    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        if (inputState.IsMenuCancel(ControllingPlayer, out PlayerIndex playerIndex))
            ResumeFromMyra();
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
