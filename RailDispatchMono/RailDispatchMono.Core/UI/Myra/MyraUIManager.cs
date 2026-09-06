using System;
using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

public sealed class MyraUIManager
{
    private bool _initialized;
    private Action? _pendingAction;
    private Widget? _previousRoot;

    public Desktop Desktop { get; private set; } = null!;
    public bool IsInitialized => _initialized;

    /// <summary>
    /// True while a temporary Myra GUI has replaced the gameplay root.
    /// </summary>
    public bool IsGameplayOverlayOpen =>
        _initialized && _previousRoot != null && Desktop.Root != _previousRoot;

    /// <summary>
    /// True for the gameplay update in which a Myra action was dispatched.
    /// This consumes the same mouse click on the UI so it cannot also reach
    /// InputManager and perform a world action.
    /// </summary>
    public bool GameplayInputConsumedThisFrame { get; private set; }

    public void Initialize(Microsoft.Xna.Framework.Game game)
    {
        if (_initialized) return;
        MyraEnvironment.Game = game;
        Desktop = new Desktop
        {
            BoundsFetcher = () => new Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height)
        };
        _initialized = true;
    }

    public void SetRoot(Widget root)
    {
        if (!_initialized) throw new InvalidOperationException("MyraUIManager must be initialized before setting a root widget.");

        // Returning to the root that was replaced closes the temporary GUI.
        // Do not overwrite _previousRoot with the GUI itself; otherwise the
        // overlay lock would remain active forever after closing the window.
        if (_previousRoot == root)
        {
            Desktop.Root = root;
            _previousRoot = null;
            return;
        }

        if (Desktop.Root != null && Desktop.Root != root)
            _previousRoot = Desktop.Root;
        Desktop.Root = root;
    }

    public void Clear()
    {
        if (!_initialized) return;
        Desktop.Root = _previousRoot;
        _previousRoot = null;
        _pendingAction = null;
        GameplayInputConsumedThisFrame = false;
    }

    public void QueueAction(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        _pendingAction += action;
    }

    public void Update(GameTime gameTime)
    {
        GameplayInputConsumedThisFrame = _pendingAction != null;
        Action? action = _pendingAction;
        _pendingAction = null;
        action?.Invoke();
    }

    public void Render()
    {
        if (!_initialized || Desktop.Root == null) return;
        Desktop.Render();
    }
}
