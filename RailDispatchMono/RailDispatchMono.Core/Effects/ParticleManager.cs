using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RailDispatchMono.Core.Effects
{
    public class ParticleManager
    {
        public void Update(GameTime gameTime) { }
        public void Draw(SpriteBatch spriteBatch) { }
        public void Emit(int count, object particleEffect) { }
        public void Emit(Vector2 position, int count = 1) { }
    }
}
