using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Shooter.Entities
{
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Zigzag,
        Kamikaze,
        Shooter
    }

    public class Enemy : Sprite
    {
        public EnemyType Type { get; set; }
        public int ScoreValue { get; set; }
        public float ShootTimer { get; set; }
        public float ShootCooldown { get; set; }
        public Vector2 Direction { get; set; }
        public float ZigzagSpeed { get; set; }
        public float ZigzagDistance { get; set; }
        private float _zigzagOffset;
        private float _zigzagStartX;

        public Enemy(Texture2D texture, EnemyType type, Vector2 position) : base(texture)
        {
            Type = type;
            Position = position;
            Direction = Vector2.Zero;
            _zigzagStartX = position.X;
            
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
                case EnemyType.Zigzag:
                    Speed = 120f;
                    ScoreValue = 25;
                    ShootCooldown = 2.5f;
                    ZigzagSpeed = 150f;
                    ZigzagDistance = 300f;
                    break;
                case EnemyType.Kamikaze:
                    Speed = 300f;
                    ScoreValue = 15;
                    ShootCooldown = 0f; // Doesn't shoot
                    break;
                case EnemyType.Shooter:
                    Speed = 80f;
                    ScoreValue = 30;
                    ShootCooldown = 1.0f;
                    break;
            }
            
            ShootTimer = ShootCooldown;
        }

        public void Update(GameTime gameTime, Rectangle bounds)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Move enemy based on type
            switch (Type)
            {
                case EnemyType.Zigzag:
                    _zigzagOffset += ZigzagSpeed * deltaTime;
                    var zigzagX = _zigzagStartX + (float)Math.Sin(_zigzagOffset / 100f) * ZigzagDistance;
                    Position = new Vector2(zigzagX, Position.Y + Speed * deltaTime);
                    break;
                    
                case EnemyType.Kamikaze:
                    // Move toward player (down and toward center)
                    var targetX = bounds.Width / 2;
                    var deltaX = targetX - Position.X;
                    var directionX = deltaX > 0 ? 1 : -1;
                    Position += new Vector2(200f * directionX * deltaTime, Speed * deltaTime);
                    break;
                    
                default:
                    // Move enemy down
                    Position += new Vector2(0, Speed * deltaTime);
                    break;
            }
            
            // Keep enemies within bounds
            Position = new Vector2(
                MathHelper.Clamp(Position.X, bounds.Left + Texture.Width / 2, bounds.Right - Texture.Width / 2),
                Position.Y
            );
            
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

