using RailDispatchMono.Core.Localization;
using RailDispatchMono.Core.UI.Myra;

namespace RailDispatchMono.Core.Screens;

internal class AboutScreen : MenuScreen
{
    private MyraAboutView? _myraView;

    public AboutScreen() : base(Resources.About)
    {
    }

    public override void LoadContent()
    {
        base.LoadContent();
        if (ScreenManager.Game is not RailDispatchMonoGame game) return;

        _myraView = new MyraAboutView(
            "RailDispatchMono — MonoGame + Myra UI",
            Resources.MonoGameSite,
            Resources.Back,
            () => LaunchDefaultBrowser("https://www.monogame.net/"),
            ExitScreen);
        game.MyraUI.SetRoot(_myraView.Root);
    }

    public override void UnloadContent()
    {
        if (ScreenManager.Game is RailDispatchMonoGame game) game.MyraUI.Clear();
        _myraView = null;
        base.UnloadContent();
    }

    public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
    {
    }

    private static void LaunchDefaultBrowser(string url)
        => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
}
