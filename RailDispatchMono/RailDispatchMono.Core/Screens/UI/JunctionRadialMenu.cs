using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.Screens.UI
{
    public class JunctionRadialMenu
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly TrackBuilder _trackBuilder;
        private Texture2D _circleTexture;
        private SpriteFont? _font;

        public bool IsOpen { get; private set; }
        public Vector2 ScreenPosition { get; private set; }
        public TrackCell? TargetTrack { get; private set; }

        private const float MenuRadius = 110f; // Promieñ roz³o¿enia menu
        private const float OptionRadius = 26f; // Promieñ pojedynczego przycisku

        private int _hoveredIndex = -1; // Indeks opcji pod kursorem myszy

        // 8 typów zwrotnic
        private static readonly JunctionType[] JunctionTypes = Enum.GetValues<JunctionType>();

        // Czytelne podgl¹dy tekstowe (Wjazd -> Odga³êzienie/G³ówny)
        private static readonly string[] JunctionLabels = new string[]
        {
            "S->NE", // 1. South_NorthEast
            "S->NW", // 2. South_NorthWest
            "W->ES", // 3. West_EastSouth
            "W->EN", // 4. West_EastNorth
            "N->SE", // 5. North_SouthEast
            "N->SW", // 6. North_SouthWest
            "E->WS", // 7. East_WestSouth
            "E->WN"  // 8. East_WestNorth
        };

        public JunctionRadialMenu(GraphicsDevice graphicsDevice, TrackBuilder trackBuilder, SpriteFont? font = null)
        {
            _graphicsDevice = graphicsDevice;
            _trackBuilder = trackBuilder;
            _font = font;
            CreateDefaultTexture();
        }

        public void SetFont(SpriteFont font)
        {
            _font = font;
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

        public void Open(Vector2 screenPosition, TrackCell targetTrack)
        {
            ScreenPosition = screenPosition;
            TargetTrack = targetTrack;
            IsOpen = true;
            _hoveredIndex = -1;
        }

        public void Close()
        {
            IsOpen = false;
            TargetTrack = null;
            _hoveredIndex = -1;
        }

        public void Update(MouseState mouse, MouseState previousMouse)
        {
            if (!IsOpen || TargetTrack == null) return;

            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            _hoveredIndex = -1;

            // Sprawdzamy, nad któr¹ opcj¹ znajduje siê kursor myszy
            for (int i = 0; i < JunctionTypes.Length; i++)
            {
                float angle = i * (MathHelper.TwoPi / JunctionTypes.Length) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                if (Vector2.Distance(mousePos, optionPos) <= OptionRadius)
                {
                    _hoveredIndex = i;
                    break;
                }
            }

            // Obs³uga klikniêcia LPM
            if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
            {
                if (_hoveredIndex != -1)
                {
                    // Przebudowujemy zwrotnicê na wybran¹ geometriê
                    _trackBuilder.BuildJunctionFromType(TargetTrack.Position, JunctionTypes[_hoveredIndex]);
                    Close();
                    return;
                }

                // Jeœli klikniêto w t³o daleko poza menu — zamykamy
                if (Vector2.Distance(mousePos, ScreenPosition) > MenuRadius + 50f)
                {
                    Close();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsOpen) return;

            // Rysowanie 8 opcji wzd³u¿ okrêgu
            for (int i = 0; i < JunctionTypes.Length; i++)
            {
                float angle = i * (MathHelper.TwoPi / JunctionTypes.Length) - MathHelper.PiOver2;
                Vector2 optionPos = ScreenPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * MenuRadius;

                bool isHovered = (i == _hoveredIndex);

                // Kolor t³a: podœwietlony na z³oty/¿ó³ty, zwyk³y na ciemnoszary
                Color buttonColor = isHovered ? Color.Gold : Color.DarkSlateGray * 0.9f;
                Color textColor = isHovered ? Color.Black : Color.White;

                // T³o przycisku
                spriteBatch.Draw(_circleTexture, optionPos - new Vector2(OptionRadius, OptionRadius), buttonColor);

                // Etykieta tekstowa z kierunkiem
                if (_font != null)
                {
                    string label = JunctionLabels[i];
                    Vector2 textSize = _font.MeasureString(label);
                    spriteBatch.DrawString(_font, label, optionPos - (textSize / 2f), textColor);
                }
            }
        }
    }
}