using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Shooter.Entities
{
    public class Player : Sprite
    {
        private const float PlayerSpeed = 600f;
        private readonly Rectangle _screenBounds;
        
        public Player(Texture2D texture, Rectangle bounds) : base(texture)
        {
            Speed = PlayerSpeed;
            _screenBounds = bounds;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var velocity = Vector2.Zero;

            // Keyboard input
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

            // Gamepad input (Left Thumbstick)
            Vector2 thumbstick = gamePadState.ThumbSticks.Left;
            velocity.X += thumbstick.X * Speed;
            velocity.Y -= thumbstick.Y * Speed; // Invert Y axis for gamepad

            Position += velocity * deltaTime;

            // Keep player on screen
            Position = new Vector2(
                MathHelper.Clamp(Position.X, Texture.Width / 2, _screenBounds.Width - Texture.Width / 2),
                MathHelper.Clamp(Position.Y, Texture.Height / 2, _screenBounds.Height - Texture.Height / 2)
            );
        }

        public Vector2 GetBulletSpawnPosition()
        {
            // Spawn bullets above the player sprite (in the direction bullets travel)
            // Position.Y is the center of the sprite, so go up by half the texture height plus some offset
            return new Vector2(Position.X, Position.Y - Texture.Height / 2 - 10);
        }
    }
}

