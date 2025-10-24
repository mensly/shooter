using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Shooter.Entities
{
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank
    }

    public class Enemy : Sprite
    {
        public EnemyType Type { get; set; }
        public int ScoreValue { get; set; }
        public float ShootTimer { get; set; }
        public float ShootCooldown { get; set; }

        public Enemy(Texture2D texture, EnemyType type, Vector2 position) : base(texture)
        {
            Type = type;
            Position = position;
            
            switch (type)
            {
                case EnemyType.Basic:
                    Speed = 100f;
                    ScoreValue = 10;
                    ShootCooldown = 2.0f;
                    break;
                case EnemyType.Fast:
                    Speed = 200f;
                    ScoreValue = 20;
                    ShootCooldown = 1.5f;
                    break;
                case EnemyType.Tank:
                    Speed = 50f;
                    ScoreValue = 50;
                    ShootCooldown = 3.0f;
                    break;
            }
            
            ShootTimer = ShootCooldown;
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Move enemy down
            Position += new Vector2(0, Speed * deltaTime);
            
            // Update shoot timer
            ShootTimer -= deltaTime;
        }

        public bool CanShoot()
        {
            return ShootTimer <= 0;
        }

        public void ResetShootTimer()
        {
            ShootTimer = ShootCooldown;
        }
    }
}

