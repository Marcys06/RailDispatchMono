using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RailDispatchMono.Core.Game.Building;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;
using System;

namespace RailDispatchMono.Core.Screens.UI;

public class JunctionRadialMenu
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly TrackBuilder _trackBuilder;
    private Texture2D _circleTexture;
    private SpriteFont? _font;

    public bool IsOpen { get; private set; }
    public Vector2 ScreenPosition { get; private set; }
    public TrackCell? TargetTrack { get; private set; }

    private const float MenuRadius = 135f;
    private const float OptionRadius = 26f;
    private int _hoveredIndex = -1;

    private static readonly JunctionType[] JunctionTypes = Enum.GetValues<JunctionType>();

    private static readonly string[] JunctionLabels =
    {
        "S->NE", "S->NW", "S->EW",
        "N->SE", "N->SW", "N->EW",
        "E->WN", "E->WS", "E->NS",
        "W->EN", "W->ES", "W->NS"
    };

    public JunctionRadialMenu(GraphicsDevice graphicsDevice, TrackBuilder trackBuilder, SpriteFont? font = null)
    {
        _graphicsDevice = graphicsDevice;
        _trackBuilder = trackBuilder;
        _font = font;
        CreateDefaultTexture();
    }

    public void SetFont(SpriteFont font) => _font = font;

    private void CreateDefaultTexture()
    {
        int diameter = (int)(OptionRadius * 2);
        _circleTexture = new Texture2D(_graphicsDevice, diameter, diameter);
        Color[] colorData = new Color[diameter * diameter];
        float radius = diameter / 2f;
        for (int y = 0; y < diameter; y++)
        for (int x = 0; x < diameter; x++)
        {
            Vector2 pos = new Vector2(x - radius, y - radius);
            colorData[y * diameter + x] = pos.Length() <= radius ? Color.White : Color.Transparent;
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
        Vector2 mousePos = new(mouse.X, mouse.Y);
        _hoveredIndex = -1;

        for (int i = 0; i < JunctionTypes.Length; i++)
        {
            float angle = i * (MathHelper.TwoPi / JunctionTypes.Length) - MathHelper.PiOver2;
            Vector2 optionPos = ScreenPosition + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * MenuRadius;
            if (Vector2.Distance(mousePos, optionPos) <= OptionRadius)
            {
                _hoveredIndex = i;
                break;
            }
        }

        if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
        {
            if (_hoveredIndex >= 0)
            {
                _trackBuilder.BuildJunctionFromType(TargetTrack.Position, JunctionTypes[_hoveredIndex]);
                Close();
            }
            else if (Vector2.Distance(mousePos, ScreenPosition) > MenuRadius + 50f)
            {
                Close();
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpen) return;
        for (int i = 0; i < JunctionTypes.Length; i++)
        {
            float angle = i * (MathHelper.TwoPi / JunctionTypes.Length) - MathHelper.PiOver2;
            Vector2 optionPos = ScreenPosition + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * MenuRadius;
            bool hovered = i == _hoveredIndex;
            Color buttonColor = hovered ? Color.Gold : Color.DarkSlateGray * 0.9f;
            Color textColor = hovered ? Color.Black : Color.White;
            spriteBatch.Draw(_circleTexture, optionPos - new Vector2(OptionRadius), buttonColor);
            if (_font != null)
            {
                string label = JunctionLabels[i];
                Vector2 textSize = _font.MeasureString(label);
                spriteBatch.DrawString(_font, label, optionPos - textSize / 2f, textColor, 0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0f);
            }
        }
    }
}
