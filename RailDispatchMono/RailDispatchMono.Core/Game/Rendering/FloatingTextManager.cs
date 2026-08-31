using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RailDispatchMono.Core.Game.Rendering;

/// <summary>Small world-space notifications used for per-wagon passenger exchange.</summary>
public sealed class FloatingTextManager
{
    private sealed class Item
    {
        public string Text = "";
        public Vector2 Position;
        public float Age;
        public float Lifetime;
    }

    private readonly List<Item> _items = new();
    private SpriteFont? _font;

    public void LoadContent(ContentManager content) => _font = content.Load<SpriteFont>("Arial24");

    public void Add(string text, Vector2 worldPosition, float lifetime = 1.75f)
    {
        _items.Add(new Item { Text = text, Position = worldPosition, Lifetime = MathF.Max(0.1f, lifetime) });
    }

    public void Update(float deltaTime)
    {
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            var item = _items[i];
            item.Age += MathF.Max(0f, deltaTime);
            item.Position.Y -= 0.35f * MathF.Max(0f, deltaTime);
            if (item.Age >= item.Lifetime) _items.RemoveAt(i);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        if (_font == null || _items.Count == 0) return;
        foreach (var item in _items)
        {
            float alpha = 1f - MathHelper.Clamp(item.Age / item.Lifetime, 0f, 1f);
            var size = _font.MeasureString(item.Text);
            var screen = camera.MapToScreen(item.Position) - new Vector2(size.X / 2f, size.Y);
            spriteBatch.DrawString(_font, item.Text, screen, Color.White * alpha);
        }
    }
}
