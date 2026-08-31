using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Effects;
using RailDispatchMono.Core.Localization;
using RailDispatchMono.Core.Settings;
using RailDispatchMono.Core.ScreenManagers;
using System.Collections.Generic;
using System.Globalization;

namespace RailDispatchMono.Core.Screens
{
    internal class SettingsScreen : MenuScreen
    {
        private MenuEntry fullscreenMenuEntry;
        private MenuEntry windowSizeMenuEntry;
        private MenuEntry languageMenuEntry;
        private MenuEntry particleEffectMenuEntry;
        private MenuEntry backMenuEntry;
        private static List<CultureInfo> languages = new();
        private static int currentLanguage;
        private GraphicsDeviceManager gdm;
        private SettingsManager<RailDispatchMonoSettings> settingsManager;
        private ParticleManager particleManager;
        private static ParticleEffectType currentParticleEffect = ParticleEffectType.Fireworks;
        private static readonly (int Width, int Height)[] WindowSizes = { (1280,720), (1600,900), (1920,1080) };

        public SettingsScreen() : base(Resources.Settings)
        {
            foreach (var culture in LocalizationManager.GetSupportedCultures()) languages.Add(culture);
            fullscreenMenuEntry = new MenuEntry(string.Empty); windowSizeMenuEntry = new MenuEntry(string.Empty); languageMenuEntry = new MenuEntry(string.Empty); particleEffectMenuEntry = new MenuEntry(string.Empty); backMenuEntry = new MenuEntry(string.Empty);
            fullscreenMenuEntry.Selected += FullScreenMenuEntrySelected; windowSizeMenuEntry.Selected += WindowSizeMenuEntrySelected; languageMenuEntry.Selected += LanguageMenuEntrySelected; particleEffectMenuEntry.Selected += ParticleEffectMenuEntrySelected; backMenuEntry.Selected += OnCancel;
            MenuEntries.Add(fullscreenMenuEntry); MenuEntries.Add(windowSizeMenuEntry); MenuEntries.Add(languageMenuEntry); MenuEntries.Add(particleEffectMenuEntry); MenuEntries.Add(backMenuEntry);
        }

        public override void LoadContent()
        {
            base.LoadContent(); gdm ??= ScreenManager.Game.Services.GetService<GraphicsDeviceManager>(); settingsManager ??= ScreenManager.Game.Services.GetService<SettingsManager<RailDispatchMonoSettings>>();
            settingsManager.Settings.PropertyChanged += (s,e)=>settingsManager.Save(); currentLanguage=settingsManager.Settings.Language; currentParticleEffect=settingsManager.Settings.ParticleEffect; gdm.IsFullScreen=settingsManager.Settings.FullScreen; gdm.PreferredBackBufferWidth=settingsManager.Settings.WindowWidth; gdm.PreferredBackBufferHeight=settingsManager.Settings.WindowHeight; gdm.ApplyChanges(); SetLanguageText(); particleManager ??= ScreenManager.Game.Services.GetService<ParticleManager>();
        }

        public override void Update(GameTime gameTime,bool otherScreenHasFocus,bool coveredByOtherScreen){base.Update(gameTime,otherScreenHasFocus,coveredByOtherScreen);particleManager.Update(gameTime);}
        public override void Draw(GameTime gameTime){SpriteBatch spriteBatch=ScreenManager.SpriteBatch;spriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,null,ScreenManager.GlobalTransformation);particleManager.Draw(spriteBatch);spriteBatch.End();base.Draw(gameTime);}

        private void SetLanguageText()
        {
            fullscreenMenuEntry.Text=string.Format(Resources.DisplayMode,gdm.IsFullScreen?Resources.FullScreen:Resources.Windowed);
            windowSizeMenuEntry.Text=$"Window: {settingsManager.Settings.WindowWidth}x{settingsManager.Settings.WindowHeight} (zmiana)";
            var selectedLanguage=languages[currentLanguage].DisplayName;if(selectedLanguage.Contains("Invariant"))selectedLanguage=Resources.English;languageMenuEntry.Text=Resources.Language+selectedLanguage;particleEffectMenuEntry.Text=Resources.ParticleEffect+currentParticleEffect;backMenuEntry.Text=Resources.Back;Title=Resources.Settings;
        }
        private void FullScreenMenuEntrySelected(object sender,PlayerIndexEventArgs e){gdm.ToggleFullScreen();settingsManager.Settings.FullScreen=gdm.IsFullScreen;}
        private void WindowSizeMenuEntrySelected(object sender,PlayerIndexEventArgs e){int index=0;for(int i=0;i<WindowSizes.Length;i++)if(WindowSizes[i].Width==settingsManager.Settings.WindowWidth&&WindowSizes[i].Height==settingsManager.Settings.WindowHeight){index=i;break;}index=(index+1)%WindowSizes.Length;var size=WindowSizes[index];settingsManager.Settings.WindowWidth=size.Width;settingsManager.Settings.WindowHeight=size.Height;gdm.PreferredBackBufferWidth=size.Width;gdm.PreferredBackBufferHeight=size.Height;gdm.ApplyChanges();SetLanguageText();}
        private void LanguageMenuEntrySelected(object sender,PlayerIndexEventArgs e){currentLanguage=(currentLanguage+1)%languages.Count;LocalizationManager.SetCulture(languages[currentLanguage].Name);settingsManager.Settings.Language=currentLanguage;SetLanguageText();}
        private void ParticleEffectMenuEntrySelected(object sender,PlayerIndexEventArgs e){currentParticleEffect++;if(currentParticleEffect>ParticleEffectType.Sparkles)currentParticleEffect=0;settingsManager.Settings.ParticleEffect=currentParticleEffect;particleManager.Emit(100,currentParticleEffect);SetLanguageText();}
    }
}
