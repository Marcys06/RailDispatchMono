using System;
using Debug = System.Diagnostics.Debug; // Alias wskazujący bezpośrednio na systemowy Debug
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RailDispatchMono.Core.Game.Map;
using RailDispatchMono.Core.Game.Railway;

namespace RailDispatchMono.Core.Game.Rendering
{
    public sealed class SignalRenderer
    {
        private readonly GameMap _map;
        private readonly SignalController _signalController;
        private Texture2D? _pixel;
        private Texture2D? _signalTexture;

        public SignalRenderer(GameMap map, SignalController signalController)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _signalController = signalController ?? throw new ArgumentNullException(nameof(signalController));
            DebugManager.Log("[SIGNAL_RENDERER] Utworzono SignalRenderer");// $2
        }

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            CreateSignalTexture(graphicsDevice);
            DebugManager.Log("[SIGNAL_RENDERER] LoadContent - tekstury utworzone");// $2
        }

        private void CreateSignalTexture(GraphicsDevice graphicsDevice)
        {
            int size = 64;
            _signalTexture = new Texture2D(graphicsDevice, size, size);
            Color[] data = new Color[size * size];
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float dist = MathF.Sqrt(dx * dx + dy * dy);

                    if (dist <= radius - 1)
                        data[y * size + x] = Color.White;
                    else
                        data[y * size + x] = Color.Transparent;
                }
            }

            _signalTexture.SetData(data);
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (_pixel is null || _signalTexture is null)
                return;

            int totalSignals = 0;
            foreach (var kvp in _signalController.Signals)
            {
                totalSignals += kvp.Value.Count;
            }

            if (totalSignals == 0)
                return;

            DebugManager.Log($"[SIGNAL_RENDERER] Rysuje {totalSignals} semaforow");// $2

            foreach (var kvp in _signalController.Signals)
            {
                foreach (var signal in kvp.Value)
                {
                    DrawSignal(spriteBatch, signal, camera);
                }
            }
        }

        private void DrawSignal(SpriteBatch spriteBatch, Signal signal, Camera camera)
        {
            // Pozycja w swiecie (World Space) - rysujemy bezposrednio na siatce kafelkow
            Vector2 worldPos = new Vector2(signal.Position.X + 0.5f, signal.Position.Y + 0.5f);
            Color color = GetSignalColor(signal.Aspect);

            // Rozmiar w jednostkach swiata (np. 0.4 kafelka)
            float worldSize = 0.4f;
            Vector2 origin = new Vector2(_signalTexture!.Width / 2f, _signalTexture.Height / 2f);
            float scale = worldSize / _signalTexture.Width;

            // 1. Rysowanie obramowania (czarne/biale tlo)
            spriteBatch.Draw(
                _signalTexture,
                worldPos,
                null,
                Color.Black,
                0f,
                origin,
                scale * 1.15f,
                SpriteEffects.None,
                0f
            );

            // 2. Rysowanie koloru sygnalu
            spriteBatch.Draw(
                _signalTexture,
                worldPos,
                null,
                color,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );

            // 3. Rysowanie znacznika / kierunku
            DrawSignalDirectionIndicator(spriteBatch, worldPos, signal.Direction);
        }

        private void DrawSignalDirectionIndicator(SpriteBatch spriteBatch, Vector2 worldCenter, TrackConnections direction)
        {
            if (_pixel is null) return;

            Vector2 offset = direction switch
            {
                TrackConnections.North => new Vector2(0, -0.25f),
                TrackConnections.South => new Vector2(0, 0.25f),
                TrackConnections.East => new Vector2(0.25f, 0),
                TrackConnections.West => new Vector2(-0.25f, 0),
                _ => Vector2.Zero
            };

            Vector2 indicatorPos = worldCenter + offset;
            float indicatorSize = 0.08f;

            spriteBatch.Draw(
                _pixel,
                indicatorPos,
                null,
                Color.White,
                0f,
                new Vector2(0.5f, 0.5f),
                new Vector2(indicatorSize, indicatorSize),
                SpriteEffects.None,
                0f
            );
        }

        public void DrawPreview(SpriteBatch spriteBatch, MapPosition position)
        {
            if (_pixel is null || _signalTexture is null)
                return;

            Vector2 worldPos = new Vector2(position.X + 0.5f, position.Y + 0.5f);
            Vector2 origin = new Vector2(_signalTexture.Width / 2f, _signalTexture.Height / 2f);
            float scale = 0.35f / _signalTexture.Width;

            spriteBatch.Draw(
                _signalTexture,
                worldPos,
                null,
                Color.LimeGreen * 0.6f,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        private Color GetSignalColor(SignalAspect aspect)
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
    }
}
