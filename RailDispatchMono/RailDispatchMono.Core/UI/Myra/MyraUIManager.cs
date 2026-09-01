using System;
using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

public sealed class MyraUIManager
{
    private bool _initialized;
    private Action? _pendingAction;

    public Desktop Desktop { get; private set; } = null!;
    public bool IsInitialized => _initialized;

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
        Desktop.Root = root;
    }

    public void Clear()
    {
        if (!_initialized) return;
        Desktop.Root = null;
        _pendingAction = null;
    }

    public void QueueAction(Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        _pendingAction += action;
    }

    public void Update(GameTime gameTime)
    {
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
