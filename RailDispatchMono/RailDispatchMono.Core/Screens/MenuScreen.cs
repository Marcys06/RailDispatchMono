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
    /// <summary>
    /// Base class for screens that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    internal abstract class MenuScreen : GameScreen
    {
        private List<MenuEntry> menuEntries = new List<MenuEntry>();
        private int selectedEntry = 0;
        private string menuTitle;
        private Color menuTitleColor = new Color(0, 0, 0);

        /// <summary>
        /// Gets or sets the title of the menu screen.
        /// </summary>
        public string Title { get => menuTitle; set => menuTitle = value; }

        /// <summary>
        /// Gets the list of menu entries, so derived classes can add
        /// or change the menu contents.
        /// </summary>
        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuScreen"/> class.
        /// </summary>
        /// <param name="menuTitle">The title of the menu screen.</param>
        public MenuScreen(string menuTitle)
        {
            this.menuTitle = menuTitle;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Loads content for the menu screen.
        /// </summary>
        public override void LoadContent()
        {
            base.LoadContent();
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or canceling the menu.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState inputState)
        {
            base.HandleInput(gameTime, inputState);

            // ✅ Sprawdź czy Font jest załadowany przed użyciem
            var font = ScreenManager?.Font;
            if (font == null) return;

            // Handle touch input for mobile platforms.
            if (RailDispatchMonoGame.IsMobile)
            {
                var touchState = inputState.CurrentTouchState;
                if (touchState.Count > 0)
                {
                    foreach (var touch in touchState)
                    {
                        if (touch.State == TouchLocationState.Pressed)
                        {
                            TextSelectedCheck(inputState.CurrentCursorLocation);
                        }
                    }
                }
            }
            // Handle mouse input for desktop platforms.
            else if (RailDispatchMonoGame.IsDesktop)
            {
                if (inputState.IsLeftMouseButtonClicked())
                {
                    TextSelectedCheck(inputState.CurrentCursorLocation);
                }
                else if (inputState.IsMiddleMouseButtonClicked())
                {
                    OnSelectEntry(selectedEntry, PlayerIndex.One);
                }
            }

            // Move to the previous menu entry.
            if (inputState.IsMenuUp(ControllingPlayer))
            {
                selectedEntry--;

                if (selectedEntry < 0)
                    selectedEntry = menuEntries.Count - 1;

                while (!menuEntries[selectedEntry].Enabled)
                {
                    selectedEntry--;

                    if (selectedEntry < 0)
                        selectedEntry = menuEntries.Count - 1;
                }
            }

            // Move to the next menu entry.
            if (inputState.IsMenuDown(ControllingPlayer))
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;

                SetNextEnabledMenu();
            }

            // Accept or cancel the menu.
            PlayerIndex playerIndex;

            if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex))
            {
                OnSelectEntry(selectedEntry, playerIndex);
            }
            else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex))
            {
                OnCancel(playerIndex);
            }
        }

        /// <summary>
        /// Checks if a touch or mouse click has selected a menu entry.
        /// </summary>
        /// <param name="touchLocation">The location of the touch or mouse click.</param>
        private void TextSelectedCheck(Vector2 touchLocation)
        {
            // ✅ Sprawdź czy Font jest załadowany
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

        /// <summary>
        /// Sets the next enabled menu entry as the selected entry.
        /// </summary>
        private void SetNextEnabledMenu()
        {
            while (!menuEntries[selectedEntry].Enabled)
            {
                selectedEntry++;

                if (selectedEntry >= menuEntries.Count)
                    selectedEntry = 0;
            }
        }

        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex, PlayerIndex playerIndex)
        {
            menuEntries[entryIndex].OnSelectEntry(playerIndex);
        }

        /// <summary>
        /// Handler for when the user has canceled the menu.
        /// </summary>
        protected virtual void OnCancel(PlayerIndex playerIndex)
        {
            ExitScreen();
        }

        /// <summary>
        /// Helper overload makes it easy to use OnCancel as a MenuEntry event handler.
        /// </summary>
        protected void OnCancel(object sender, PlayerIndexEventArgs e)
        {
            OnCancel(e.PlayerIndex);
        }

        /// <summary>
        /// Updates the positions of the menu entries.
        /// </summary>
        protected virtual void UpdateMenuEntryLocations()
        {
            DebugManager.Log("[MENU] UpdateMenuEntryLocations() - START");

            var font = ScreenManager?.Font;
            if (font == null)
            {
                DebugManager.Log("[MENU] UpdateMenuEntryLocations() - BRAK FONT!");
                return;
            }

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

                DebugManager.Log($"[MENU] Entry {i}: '{menuEntry.Text}' -> Position: {position}");

                position.Y += menuEntry.GetHeight(this);
            }

            DebugManager.Log("[MENU] UpdateMenuEntryLocations() - KONIEC");
        }

        /// <summary>
        /// Updates the menu screen.
        /// </summary>
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

        /// <summary>
        /// Draws the menu screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            DebugManager.Log("[MENU] 🔥 Draw() - START");

            var font = ScreenManager?.Font;
            var spriteBatch = ScreenManager?.SpriteBatch;
            var graphics = ScreenManager?.GraphicsDevice;

            DebugManager.Log($"[MENU] Font: {font != null}, SpriteBatch: {spriteBatch != null}");

            if (font == null || spriteBatch == null || graphics == null)
            {
                DebugManager.Log("[MENU] ❌ BRAK ZASOBÓW - nie rysuję!");
                return;
            }

            DebugManager.Log($"[MENU] Liczba entries: {menuEntries.Count}");

            // Make sure our entries are in the right place before we draw them.
            UpdateMenuEntryLocations();

            DebugManager.Log("[MENU] Po UpdateMenuEntryLocations()");

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];
                bool isSelected = IsActive && (i == selectedEntry);

                DebugManager.Log($"[MENU] Rysuję entry {i}: '{menuEntry.Text}' at {menuEntry.Position}");

                menuEntry.Draw(this, isSelected, gameTime);
            }

            // Rysuj tytuł
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);
            Vector2 titlePosition = new Vector2(ScreenManager.BaseScreenSize.X / 2, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            Color titleColor = menuTitleColor * TransitionAlpha;
            float titleScale = 1.25f;
            titlePosition.Y -= transitionOffset * 100;

            DebugManager.Log($"[MENU] Tytuł: '{menuTitle}' at {titlePosition}");

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            spriteBatch.End();

            DebugManager.Log("[MENU] 🔥 Draw() - KONIEC");
        }
    }
}