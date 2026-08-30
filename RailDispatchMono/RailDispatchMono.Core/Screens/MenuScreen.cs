using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using RailDispatchMono.Core;
using RailDispatchMono.Core.Inputs;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Screens
{
    internal abstract class MenuScreen : GameScreen
    {
        private List<MenuEntry> menuEntries = new List<MenuEntry>();
        private int selectedEntry = 0;
        private string menuTitle;
        private Color menuTitleColor = new Color(0, 0, 0);

        public string Title { get => menuTitle; set => menuTitle = value; }
        protected IList<MenuEntry> MenuEntries => menuEntries;

        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent() => base.LoadContent();

        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            base.HandleInput(gameTime, inputState);
            var font = ScreenManager?.Font;
            if (font == null) return;

            if (RailDispatchMonoGame.IsMobile)
            {
                var touchState = inputState.CurrentTouchState;
                if (touchState.Count > 0)
                {
                    foreach (var touch in touchState)
                    {
                        if (touch.State == TouchLocationState.Pressed)
                            TextSelectedCheck(inputState.CurrentCursorLocation);
                    }
                }
            }
            else if (RailDispatchMonoGame.IsDesktop)
            {
                if (inputState.IsLeftMouseButtonClicked())
                    TextSelectedCheck(inputState.CurrentCursorLocation);
                else if (inputState.IsMiddleMouseButtonClicked())
                    OnSelectEntry(selectedEntry, PlayerIndex.One);
            }

            if (inputState.IsMenuUp(ControllingPlayer))
            {
                selectedEntry--;
                if (selectedEntry < 0) selectedEntry = menuEntries.Count - 1;
                while (!menuEntries[selectedEntry].Enabled)
                {
                    selectedEntry--;
                    if (selectedEntry < 0) selectedEntry = menuEntries.Count - 1;
                }
            }

            if (inputState.IsMenuDown(ControllingPlayer))
            {
                selectedEntry++;
                if (selectedEntry >= menuEntries.Count) selectedEntry = 0;
                SetNextEnabledMenu();
            }

            PlayerIndex playerIndex;
            if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex))
                OnSelectEntry(selectedEntry, playerIndex);
            else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex))
                OnCancel(playerIndex);
        }

        private void TextSelectedCheck(Vector2 touchLocation)
        {
            var font = ScreenManager?.Font;
            if (font == null) return;

            for (int i = 0; i < menuEntries.Count; i++)
            {
                var textSize = font.MeasureString(menuEntries[i].Text);
                var entryBounds = new Rectangle((int)menuEntries[i].Position.X, (int)menuEntries[i].Position.Y, (int)textSize.X, (int)textSize.Y);
                if (entryBounds.Contains(touchLocation))
                {
                    selectedEntry = i;
                    OnSelectEntry(selectedEntry, ControllingPlayer ?? PlayerIndex.One);
                    break;
                }
            }
        }

        private void SetNextEnabledMenu()
        {
            while (!menuEntries[selectedEntry].Enabled)
            {
                selectedEntry++;
                if (selectedEntry >= menuEntries.Count) selectedEntry = 0;
            }
        }

        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex) => menuEntries[entryIndex].OnSelectEntry(playerIndex);
        protected virtual void OnCancel(PlayerIndex playerIndex) => ExitScreen();
        protected void OnCancel(object sender, PlayerIndexEventArgs e) => OnCancel(e.PlayerIndex);

        protected virtual void UpdateMenuEntryLocations()
        {
            var font = ScreenManager?.Font;
            if (font == null) return;

            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);
            Vector2 position = new Vector2(0f, 175f);

            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];
                position.X = ScreenManager.BaseScreenSize.X / 2 - menuEntry.GetWidth(this) / 2;
                if (ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset * 256;
                else
                    position.X += transitionOffset * 512;
                menuEntry.Position = position;
                position.Y += menuEntry.GetHeight(this);
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            SetNextEnabledMenu();
            for (int i = 0; i < menuEntries.Count; i++)
            {
                bool isSelected = IsActive && (i == selectedEntry);
                menuEntries[i].Update(this, isSelected, gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var font = ScreenManager?.Font;
            var spriteBatch = ScreenManager?.SpriteBatch;
            var graphics = ScreenManager?.GraphicsDevice;
            if (font == null || spriteBatch == null || graphics == null) return;

            UpdateMenuEntryLocations();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];
                bool isSelected = IsActive && (i == selectedEntry);
                menuEntry.Draw(this, isSelected, gameTime);
            }

            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);
            Vector2 titlePosition = new Vector2(ScreenManager.BaseScreenSize.X / 2, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = menuTitleColor * TransitionAlpha;
            float titleScale = 1.25f;
            titlePosition.Y -= transitionOffset * 100;
            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0, titleOrigin, titleScale, SpriteEffects.None, 0);
            spriteBatch.End();
        }
    }
}