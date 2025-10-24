using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shooter.Managers
{
    public class EnemyManager
    {
        private const float SpawnCooldown = 1.2f; // More frequent spawning
        
        private List<Enemy> _enemies;
        private Texture2D _basicEnemyTexture;
        private Texture2D _fastEnemyTexture;
        private Texture2D _tankEnemyTexture;
        private Texture2D _zigzagEnemyTexture;
        private Texture2D _kamikazeEnemyTexture;
        private Texture2D _shooterEnemyTexture;
        private float _spawnTimer;
        private Rectangle _screenBounds;
        private Random _random;
        private BulletManager _bulletManager;
        private SoundEffect _enemyShootSound;

        public EnemyManager(ContentManager content, Rectangle bounds, BulletManager bulletManager, SoundEffect enemyShootSound)
        {
            _enemies = new List<Enemy>();
            _random = new Random();
            _screenBounds = bounds;
            _bulletManager = bulletManager;
            _enemyShootSound = enemyShootSound;
            
            _basicEnemyTexture = content.Load<Texture2D>("enemy_basic");
            _fastEnemyTexture = content.Load<Texture2D>("enemy_fast");
            _tankEnemyTexture = content.Load<Texture2D>("enemy_tank");
            _zigzagEnemyTexture = content.Load<Texture2D>("enemy_zigzag");
            _kamikazeEnemyTexture = content.Load<Texture2D>("enemy_kamikaze");
            _shooterEnemyTexture = content.Load<Texture2D>("enemy_shooter");
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            _spawnTimer -= deltaTime;
            
            if (_spawnTimer <= 0)
            {
                SpawnEnemy();
                _spawnTimer = SpawnCooldown;
            }
            
            foreach (var enemy in _enemies.ToList())
            {
                enemy.Update(gameTime, _screenBounds);
                
                // Handle enemy shooting
                if (enemy.CanShoot())
                {
                    // Spawn bullets below the enemy (ahead as they move down)
                    var bulletSpawnPos = new Vector2(enemy.Position.X, enemy.Position.Y + enemy.Texture.Height / 2 + 10);
                    _bulletManager.AddEnemyBullet(bulletSpawnPos);
                    _enemyShootSound.Play();
                    enemy.ResetShootTimer(_random);
                }
                
                // Remove enemy if off screen
                if (enemy.Position.Y > _screenBounds.Height + 50)
                {
                    _enemies.Remove(enemy);
                }
            }
        }

        private void SpawnEnemy()
        {
            var randX = _random.Next(100, _screenBounds.Width - 100);
            var enemyType = (EnemyType)_random.Next(0, 6); // 6 enemy types now
            
            Texture2D texture = enemyType switch
            {
                EnemyType.Basic => _basicEnemyTexture,
                EnemyType.Fast => _fastEnemyTexture,
                EnemyType.Tank => _tankEnemyTexture,
                EnemyType.Zigzag => _zigzagEnemyTexture,
                EnemyType.Kamikaze => _kamikazeEnemyTexture,
                EnemyType.Shooter => _shooterEnemyTexture,
                _ => _basicEnemyTexture
            };
            
            _enemies.Add(new Enemy(texture, enemyType, new Vector2(randX, -50), _random));
        }

        public Enemy CheckBulletCollision(Rectangle bulletBounds)
        {
            foreach (var enemy in _enemies.ToList())
            {
                if (enemy.Bounds.Intersects(bulletBounds))
                {
                    _enemies.Remove(enemy);
                    return enemy;
                }
            }
            return null;
        }

        public bool CheckPlayerCollision(Rectangle playerBounds)
        {
            return _enemies.Any(e => e.Bounds.Intersects(playerBounds));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var enemy in _enemies)
            {
                enemy.Draw(spriteBatch);
            }
        }

        public void Reset()
        {
            _enemies.Clear();
            _spawnTimer = 0;
        }
    }
}

