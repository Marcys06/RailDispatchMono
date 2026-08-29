// ============================================================
// SIGNALRADIALMENU.CS - MENU RADIALNE DLA SEMAFORÓW
// ============================================================

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Screens.UI
{
    public class SignalRadialMenu
    {
        private readonly GraphicsDevice _graphicsDevice;
        private Texture2D _circleTexture;
        private SpriteFont? _font;

        public bool IsOpen { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public Signal? CurrentSignal { get; private set; }

        private const float MenuRadius = 110f;
        private const float OptionRadius = 26f;
        private int _hoveredIndex = -1;
        private List<SignalAspect> _aspects = new();

        public event EventHandler<SignalAspect>? AspectSelected;
        public event EventHandler? MenuClosed;

        public SignalRadialMenu(GraphicsDevice graphicsDevice, SpriteFont? font = null)
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

        public void Open(Vector2 screenPosition, Signal signal)
        {
            ScreenPosition = screenPosition;
            CurrentSignal = signal;

            // ✅ TWORZYMY KOPIĘ LISTY - NIE REFERENCJĘ
            _aspects = new List<SignalAspect>(signal.AvailableAspects);

            System.Diagnostics.Debug.WriteLine($"[RADIAL] Otwieram menu dla semafora na {signal.Position}");
            System.Diagnostics.Debug.WriteLine($"[RADIAL] Dostepne aspekty: {_aspects.Count}");

            if (_aspects.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[RADIAL] UWAGA: Brak aspektow! Dodaje domyslne.");
                _aspects = new List<SignalAspect>
        {
            SignalAspect.Stop,
            SignalAspect.Clear,
            SignalAspect.Warning,
            SignalAspect.Speed100,
            SignalAspect.Speed40,
            SignalAspect.StopStation,
            SignalAspect.Reserve1,
            SignalAspect.Reserve2,
            SignalAspect.Reserve3,
            SignalAspect.Reserve4
        };
            }
            else
            {
                foreach (var a in _aspects)
                {
                    System.Diagnostics.Debug.WriteLine($"[RADIAL] - {a}");
                }
            }

            IsOpen = true;
            _hoveredIndex = -1;
        }

        public void Close()
        {
            IsOpen = false;
            CurrentSignal = null;
            _aspects.Clear();  // ✅ Czyści KOPIĘ
            _hoveredIndex = -1;
            MenuClosed?.Invoke(this, EventArgs.Empty);
            System.Diagnostics.Debug.WriteLine("[RADIAL] Menu zamkniete");
        }

        public void Update(MouseState mouse, MouseState previousMouse)
        {
            if (!IsOpen || _aspects.Count == 0) return;

            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            _hoveredIndex = -1;

            for (int i = 0; i < _aspects.Count; i++)
            {
                float angle = i * (MathHelper.TwoPi / _aspects.Count) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                if (Vector2.Distance(mousePos, optionPos) <= OptionRadius)
                {
                    _hoveredIndex = i;
                    break;
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (_hoveredIndex != -1 && _hoveredIndex < _aspects.Count)
                {
                    var selectedAspect = _aspects[_hoveredIndex];
                    System.Diagnostics.Debug.WriteLine($"[RADIAL] Wybrano aspekt: {selectedAspect}");
                    CurrentSignal?.SetAspect(selectedAspect);
                    AspectSelected?.Invoke(this, selectedAspect);
                    Close();
                    return;
                }

                if (Vector2.Distance(mousePos, ScreenPosition) > MenuRadius + 50f)
                {
                    System.Diagnostics.Debug.WriteLine("[RADIAL] Kliknieto poza menu - zamykam");
                    Close();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOpen || _aspects.Count == 0) return;

            for (int i = 0; i < _aspects.Count; i++)
            {
                float angle = i * (MathHelper.TwoPi / _aspects.Count) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                bool isHovered = (i == _hoveredIndex);
                Color color = GetAspectColor(_aspects[i]);
                Color buttonColor = isHovered ? Color.Gold : color * 0.8f;
                Color textColor = isHovered ? Color.Black : Color.White;

                spriteBatch.Draw(_circleTexture, optionPos - new Vector2(OptionRadius, OptionRadius), buttonColor);

                if (_font != null)
                {
                    string label = GetAspectLabel(_aspects[i]);
                    Vector2 textSize = _font.MeasureString(label);
                    spriteBatch.DrawString(_font, label, optionPos - (textSize / 2f), textColor);
                }
            }
        }

        private Color GetAspectColor(SignalAspect aspect)
        {
            return aspect switch
            {
                SignalAspect.Stop => Color.Red,
                SignalAspect.StopStation => Color.DarkRed,
                SignalAspect.Clear => Color.LimeGreen,
                SignalAspect.Warning => Color.Yellow,
                SignalAspect.Speed100 => Color.Orange,
                SignalAspect.Speed40 => Color.OrangeRed,
                SignalAspect.Reserve1 => Color.Cyan,
                SignalAspect.Reserve2 => Color.Blue,
                SignalAspect.Reserve3 => Color.Purple,
                SignalAspect.Reserve4 => Color.Magenta,
                _ => Color.Gray
            };
        }

        private string GetAspectLabel(SignalAspect aspect)
        {
            return SignalAspectInfo.Aspects.TryGetValue(aspect, out var info)
                ? info.Name
                : aspect.ToString();
        }
    }
}