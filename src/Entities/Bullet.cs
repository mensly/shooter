using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Entities
{
    public class Bullet : Sprite
    {
        private const float PlayerBulletSpeed = 500f;
        private const float EnemyBulletSpeed = 300f;
        
        public bool IsActive { get; set; }
        public bool IsPlayerBullet { get; set; }
        private readonly Rectangle _bounds;

        public Bullet(Texture2D texture, Vector2 position, bool isPlayerBullet, Rectangle bounds) : base(texture)
        {
            Position = position;
            IsPlayerBullet = isPlayerBullet;
            IsActive = true;
            Speed = isPlayerBullet ? PlayerBulletSpeed : EnemyBulletSpeed;
            _bounds = bounds;
        }

        public void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var direction = IsPlayerBullet ? -1 : 1;
            Position += new Vector2(0, Speed * direction * deltaTime);

            // Deactivate if off screen
            if (Position.Y < -Texture.Height || Position.Y > _bounds.Height + Texture.Height)
            {
                IsActive = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                base.Draw(spriteBatch);
            }
        }
    }
}

