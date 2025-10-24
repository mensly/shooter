using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Entities
{
    public class Sprite
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public float Speed { get; set; }
        public Rectangle Bounds => new Rectangle(
            (int)Position.X - Texture.Width / 2,
            (int)Position.Y - Texture.Height / 2,
            Texture.Width,
            Texture.Height
        );

        public Sprite(Texture2D texture)
        {
            Texture = texture;
            Speed = 0;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0f, 
                new Vector2(Texture.Width / 2, Texture.Height / 2), 1f, 
                SpriteEffects.None, 0f);
        }
    }
}

