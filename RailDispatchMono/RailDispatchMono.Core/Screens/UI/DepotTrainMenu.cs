using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace RailDispatchMono.Core.Screens.UI;

/// <summary>Consist selection shown after clicking a depot.</summary>
public sealed class DepotTrainMenu
{
    public readonly record struct ConsistPreset(string Name, int PassengerWagons);

    public static readonly ConsistPreset[] Presets =
    {
        new("Krótki — lokomotywa + 1 wagon", 1),
        new("Standard — lokomotywa + 2 wagony", 2),
        new("Długi — lokomotywa + 4 wagony", 4)
    };

    private readonly GraphicsDevice _graphicsDevice;
    private SpriteFont? _font;
    private Texture2D? _pixel;
    private MouseState _previousMouse;
    private Vector2 _position;

    public bool IsOpen { get; private set; }
    public int SelectedIndex { get; private set; } = 1;
    public event Action<int>? ConsistSelected;

    public DepotTrainMenu(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _previousMouse = Mouse.GetState();
    }

    public void SetFont(SpriteFont font) => _font = font;
    public void LoadContent()
    {
        if (_pixel != null) return;
        _pixel = new Texture2D(_graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Open(Vector2 screenPosition)
    {
        IsOpen = true;
        _position = screenPosition;
        _previousMouse = Mouse.GetState();
        Clamp();
    }

    public void Close() => IsOpen = false;

    public void Update(MouseState mouse)
    {
        if (!IsOpen) return;
        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            for (int i = 0; i < Presets.Length; i++)
            {
                if (!ButtonRect(i).Contains(mouse.Position)) continue;
                SelectedIndex = i;
                IsOpen = false;
                ConsistSelected?.Invoke(Presets[i].PassengerWagons);
                break;
            }
            if (CloseRect().Contains(mouse.Position)) IsOpen = false;
        }
        if (mouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released) IsOpen = false;
        _previousMouse = mouse;
    }

    public void Draw(SpriteBatch batch)
    {
        if (!IsOpen || _font == null || _pixel == null) return;
        Clamp();
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        batch.Draw(_pixel, new Rectangle((int)_position.X, (int)_position.Y, 390, 220), new Color(18, 18, 18, 248));
        batch.Draw(_pixel, new Rectangle((int)_position.X, (int)_position.Y, 390, 3), Color.Yellow);
        batch.DrawString(_font, "WYBIERZ ZESTAWIENIE", _position + new Vector2(16, 12), Color.Yellow, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
        for (int i = 0; i < Presets.Length; i++)
        {
            var rect = ButtonRect(i);
            batch.Draw(_pixel, rect, i == SelectedIndex ? new Color(70, 90, 35, 245) : new Color(45, 45, 45, 245));
            batch.DrawString(_font, Presets[i].Name, new Vector2(rect.X + 10, rect.Y + 9), Color.White, 0f, Vector2.Zero, 0.55f, SpriteEffects.None, 0f);
        }
        var close = CloseRect();
        batch.Draw(_pixel, close, new Color(45, 45, 45, 245));
        batch.DrawString(_font, "ANULUJ", new Vector2(close.X + 10, close.Y + 7), Color.White, 0f, Vector2.Zero, 0.55f, SpriteEffects.None, 0f);
        batch.End();
    }

    private Rectangle ButtonRect(int index) => new((int)_position.X + 15, (int)_position.Y + 48 + index * 38, 360, 32);
    private Rectangle CloseRect() => new((int)_position.X + 285, (int)_position.Y + 171, 90, 32);
    private void Clamp()
    {
        _position.X = MathHelper.Clamp(_position.X, 4, Math.Max(4, _graphicsDevice.Viewport.Width - 394));
        _position.Y = MathHelper.Clamp(_position.Y, 4, Math.Max(4, _graphicsDevice.Viewport.Height - 224));
    }
}
