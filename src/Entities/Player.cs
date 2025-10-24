using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shooter.Entities
{
    public class Player : Sprite
    {
        private readonly Rectangle _screenBounds;
        
        public Player(Texture2D texture) : base(texture)
        {
            Speed = 300f;
            _screenBounds = new Rectangle(0, 0, 1024, 768);
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var velocity = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
            {
                velocity.X -= Speed;
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                velocity.X += Speed;
            }
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                velocity.Y -= Speed;
            }
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                velocity.Y += Speed;
            }

            Position += velocity * deltaTime;

            // Keep player on screen
            Position = new Vector2(
                MathHelper.Clamp(Position.X, Texture.Width / 2, _screenBounds.Width - Texture.Width / 2),
                MathHelper.Clamp(Position.Y, Texture.Height / 2, _screenBounds.Height - Texture.Height / 2)
            );
        }

        public Vector2 GetBulletSpawnPosition()
        {
            return new Vector2(Position.X, Position.Y - Texture.Height / 2);
        }
    }
}

