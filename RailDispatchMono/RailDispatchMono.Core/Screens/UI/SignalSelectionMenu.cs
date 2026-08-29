// ============================================================
// SIGNALSELECTIONMENU.CS - MENU WYBORU SEMAFORA
// ============================================================

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Screens.UI
{
    public class SignalSelectionMenu
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D _circleTexture;
        private SpriteFont? _font;

        public bool IsOpen { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public List<Signal> Signals { get; private set; } = new();

        private const float MenuRadius = 80f;
        private const float OptionRadius = 30f;
        private int _hoveredIndex = -1;

        public event EventHandler<Signal>? SignalSelected;
        public event EventHandler? MenuClosed;

        public SignalSelectionMenu(GraphicsDevice graphicsDevice, SpriteFont? font = null)
        {
            _graphicsDevice = graphicsDevice;
            _font = font;
            CreateDefaultTexture();
        }

        private void CreateDefaultTexture()
        {
            int diameter = (int)(OptionRadius * 2);
            _circleTexture = new Texture2D(_graphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];
            float radius = diameter / 2f;

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    Vector2 pos = new Vector2(x - radius, y - radius);
                    if (pos.Length() <= radius)
                        colorData[y * diameter + x] = Color.White;
                    else
                        colorData[y * diameter + x] = Color.Transparent;
                }
            }
            _circleTexture.SetData(colorData);
        }

        public void SetFont(SpriteFont font)
        {
            _font = font;
        }

        public void Open(Vector2 screenPosition, List<Signal> signals)
        {
            ScreenPosition = screenPosition;
            Signals = signals;
            IsOpen = true;
            _hoveredIndex = -1;
        }

        public void Close()
        {
            IsOpen = false;
            Signals.Clear();
            _hoveredIndex = -1;
            MenuClosed?.Invoke(this, EventArgs.Empty);
        }

        public void Update(MouseState mouse, MouseState previousMouse)
        {
            if (!IsOpen || Signals.Count == 0) return;

            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            _hoveredIndex = -1;

            for (int i = 0; i < Signals.Count; i++)
            {
                float angle = i * (MathHelper.TwoPi / Signals.Count) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                if (Vector2.Distance(mousePos, optionPos) <= OptionRadius)
                {
                    _hoveredIndex = i;
                    break;
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (_hoveredIndex != -1 && _hoveredIndex < Signals.Count)
                {
                    var selectedSignal = Signals[_hoveredIndex];
                    SignalSelected?.Invoke(this, selectedSignal);
                    Close();
                    return;
                }

                if (Vector2.Distance(mousePos, ScreenPosition) > MenuRadius + 50f)
                {
                    Close();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOpen || Signals.Count == 0) return;

            for (int i = 0; i < Signals.Count; i++)
            {
                float angle = i * (MathHelper.TwoPi / Signals.Count) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                bool isHovered = (i == _hoveredIndex);
                Color color = isHovered ? Color.Gold : Color.DarkSlateGray * 0.9f;
                Color textColor = isHovered ? Color.Black : Color.White;

                spriteBatch.Draw(_circleTexture, optionPos - new Vector2(OptionRadius, OptionRadius), color);

                if (_font != null)
                {
                    string label = GetSignalLabel(Signals[i]);
                    Vector2 textSize = _font.MeasureString(label);
                    spriteBatch.DrawString(_font, label, optionPos - (textSize / 2f), textColor);
                }
            }
        }

        private string GetSignalLabel(Signal signal)
        {
            string direction = signal.Direction switch
            {
                TrackConnections.North => "↑",
                TrackConnections.South => "↓",
                TrackConnections.East => "→",
                TrackConnections.West => "←",
                _ => "?"
            };

            return $"{direction} {signal.GetAspectName()}";
        }
    }
}