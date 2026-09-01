using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;

namespace RailDispatchMono.Core.UI.Myra;

/// <summary>
/// Owns the shared Myra desktop used by RailDispatchMono UI screens.
/// Screen lifecycle remains owned by <see cref="ScreenManagers.ScreenManager"/>.
/// </summary>
public sealed class MyraUIManager
{
    private bool _initialized;

    public Desktop Desktop { get; private set; } = null!;

    public bool IsInitialized => _initialized;

    public void Initialize(Game game)
    {
        if (_initialized)
            return;

        MyraEnvironment.Game = game;
        Desktop = new Desktop();
        _initialized = true;
    }

    public void Render()
    {
        if (!_initialized)
            return;

        Desktop.Render();
    }
}
