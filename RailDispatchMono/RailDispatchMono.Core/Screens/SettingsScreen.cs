using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Effects;
using RailDispatchMono.Core.Localization;
using RailDispatchMono.Core.Settings;
using RailDispatchMono.Core.UI.Myra;
using System.Collections.Generic;
using System.Globalization;

namespace RailDispatchMono.Core.Screens;

internal class SettingsScreen : MenuScreen
{
    private static readonly List<CultureInfo> Languages = new();
    private static int _currentLanguage;
    private static ParticleEffectType _currentParticleEffect = ParticleEffectType.Fireworks;
    private static readonly (int Width, int Height)[] WindowSizes = { (1280, 720), (1600, 900), (1920, 1080) };
    private GraphicsDeviceManager? _gdm;
    private SettingsManager<RailDispatchMonoSettings>? _settingsManager;
    private ParticleManager? _particleManager;
    private MyraSettingsView? _myraView;

    public SettingsScreen() : base(Resources.Settings)
    {
        if (Languages.Count == 0)
            Languages.AddRange(LocalizationManager.GetSupportedCultures());
    }

    public override void LoadContent()
    {
        base.LoadContent();
        _gdm ??= ScreenManager.Game.Services.GetService<GraphicsDeviceManager>();
        _settingsManager ??= ScreenManager.Game.Services.GetService<SettingsManager<RailDispatchMonoSettings>>();
        _particleManager ??= ScreenManager.Game.Services.GetService<ParticleManager>();
        if (_settingsManager == null || _gdm == null) return;
        _settingsManager.Settings.PropertyChanged += SettingsChanged;
        _currentLanguage = _settingsManager.Settings.Language;
        _currentParticleEffect = _settingsManager.Settings.ParticleEffect;
        _gdm.IsFullScreen = _settingsManager.Settings.FullScreen;
        _gdm.PreferredBackBufferWidth = _settingsManager.Settings.WindowWidth;
        _gdm.PreferredBackBufferHeight = _settingsManager.Settings.WindowHeight;
        _gdm.ApplyChanges();
        RefreshView();
    }

    public override void UnloadContent()
    {
        if (_settingsManager != null) _settingsManager.Settings.PropertyChanged -= SettingsChanged;
        if (ScreenManager.Game is RailDispatchMonoGame game) game.MyraUI.Clear();
        _myraView = null;
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        _particleManager?.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        if (_particleManager == null) return;
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);
        _particleManager.Draw(spriteBatch);
        spriteBatch.End();
    }

    private void SettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        _settingsManager?.Save();
        RefreshView();
    }

    private void RefreshView()
    {
        if (_settingsManager == null || _gdm == null || ScreenManager.Game is not RailDispatchMonoGame game) return;
        string selectedLanguage = Languages.Count == 0 ? Resources.English : Languages[_currentLanguage % Languages.Count].DisplayName;
        if (selectedLanguage.Contains("Invariant")) selectedLanguage = Resources.English;
        _myraView = new MyraSettingsView(
            string.Format(Resources.DisplayMode, _gdm.IsFullScreen ? Resources.FullScreen : Resources.Windowed),
            $"Window: {_settingsManager.Settings.WindowWidth}x{_settingsManager.Settings.WindowHeight}",
            Resources.Language + selectedLanguage,
            Resources.ParticleEffect + _currentParticleEffect,
            Resources.Back,
            ToggleFullscreen, CycleWindowSize, CycleLanguage, CycleParticleEffect, Back);
        game.MyraUI.SetRoot(_myraView.Root);
    }

    private void ToggleFullscreen()
    {
        if (_gdm == null || _settingsManager == null) return;
        _gdm.ToggleFullScreen();
        _settingsManager.Settings.FullScreen = _gdm.IsFullScreen;
        RefreshView();
    }

    private void CycleWindowSize()
    {
        if (_gdm == null || _settingsManager == null) return;
        int index = 0;
        for (int i = 0; i < WindowSizes.Length; i++)
            if (WindowSizes[i].Width == _settingsManager.Settings.WindowWidth && WindowSizes[i].Height == _settingsManager.Settings.WindowHeight) { index = i; break; }
        index = (index + 1) % WindowSizes.Length;
        var size = WindowSizes[index];
        _settingsManager.Settings.WindowWidth = size.Width;
        _settingsManager.Settings.WindowHeight = size.Height;
        _gdm.PreferredBackBufferWidth = size.Width;
        _gdm.PreferredBackBufferHeight = size.Height;
        _gdm.ApplyChanges();
        RefreshView();
    }

    private void CycleLanguage()
    {
        if (_settingsManager == null || Languages.Count == 0) return;
        _currentLanguage = (_currentLanguage + 1) % Languages.Count;
        LocalizationManager.SetCulture(Languages[_currentLanguage].Name);
        _settingsManager.Settings.Language = _currentLanguage;
        RefreshView();
    }

    private void CycleParticleEffect()
    {
        if (_settingsManager == null) return;
        _currentParticleEffect++;
        if (_currentParticleEffect > ParticleEffectType.Sparkles) _currentParticleEffect = 0;
        _settingsManager.Settings.ParticleEffect = _currentParticleEffect;
        _particleManager?.Emit(100, _currentParticleEffect);
        RefreshView();
    }

    private void Back() => ExitScreen();
}
