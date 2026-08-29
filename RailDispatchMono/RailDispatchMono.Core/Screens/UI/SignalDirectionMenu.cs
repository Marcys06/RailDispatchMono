// ============================================================
// SIGNALDIRECTIONMENU.CS - MENU WYBORU KIERUNKU
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Screens.UI
{
    public class SignalDirectionMenu : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D? _circleTexture;
        private SpriteFont? _font;

        public bool IsOpen { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public MapPosition TargetPosition { get; private set; }
        public List<TrackConnections> AvailableDirections { get; private set; } = new();

        private const float MenuRadius = 80f;
        private const float OptionRadius = 30f;
        private int _hoveredIndex = -1;

        // ZDARZENIA
        public event EventHandler<SignalDirectionSelectedEventArgs>? DirectionSelected;
        public event EventHandler? MenuClosed;

        public class SignalDirectionSelectedEventArgs : EventArgs
        {
            public MapPosition Position { get; }
            public TrackConnections Direction { get; }

            public SignalDirectionSelectedEventArgs(MapPosition position, TrackConnections direction)
            {
                Position = position;
                Direction = direction;
            }
        }

        public SignalDirectionMenu(GraphicsDevice graphicsDevice, SpriteFont? font = null)
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

        public void Open(Vector2 screenPosition, MapPosition mapPos, List<TrackConnections> directions)
        {
            ScreenPosition = screenPosition;
            TargetPosition = mapPos;
            AvailableDirections = directions ?? new List<TrackConnections>();
            IsOpen = true;
            _hoveredIndex = -1;
        }

        public void Close()
        {
            IsOpen = false;
            AvailableDirections.Clear();
            _hoveredIndex = -1;
            MenuClosed?.Invoke(this, EventArgs.Empty);
        }

        public void Update(MouseState mouse, MouseState previousMouse)
        {
            if (!IsOpen || AvailableDirections.Count == 0) return;

            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            _hoveredIndex = -1;

            for (int i = 0; i < AvailableDirections.Count; i++)
            {
                float angle = i * (MathHelper.TwoPi / AvailableDirections.Count) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                if (Vector2.Distance(mousePos, optionPos) <= OptionRadius)
                {
                    _hoveredIndex = i;
                    break;
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (_hoveredIndex != -1 && _hoveredIndex < AvailableDirections.Count)
                {
                    var selectedDirection = AvailableDirections[_hoveredIndex];
                    DirectionSelected?.Invoke(this, new SignalDirectionSelectedEventArgs(TargetPosition, selectedDirection));
                    Close();
                    return;
                }

                // Kliknięcie poza menu zamyka je
                if (Vector2.Distance(mousePos, ScreenPosition) > MenuRadius + 50f)
                {
                    Close();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOpen || AvailableDirections.Count == 0 || _circleTexture == null) return;

            for (int i = 0; i < AvailableDirections.Count; i++)
            {
                float angle = i * (MathHelper.TwoPi / AvailableDirections.Count) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                bool isHovered = (i == _hoveredIndex);
                Color color = isHovered ? Color.Gold : Color.DarkSlateGray * 0.9f;
                Color textColor = isHovered ? Color.Black : Color.White;

                spriteBatch.Draw(_circleTexture, optionPos - new Vector2(OptionRadius, OptionRadius), color);

                if (_font != null)
                {
                    string label = GetDirectionLabel(AvailableDirections[i]);
                    Vector2 textSize = _font.MeasureString(label);
                    spriteBatch.DrawString(_font, label, optionPos - (textSize / 2f), textColor);
                }
            }
        }

        private string GetDirectionLabel(TrackConnections direction)
        {
            return direction switch
            {
                TrackConnections.North => "N",
                TrackConnections.South => "S",
                TrackConnections.East => "E",
                TrackConnections.West => "W",
                _ => "?"
            };
        }

        public void Dispose()
        {
            _circleTexture?.Dispose();
            _circleTexture = null;
        }
    }
}