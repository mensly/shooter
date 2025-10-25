using Microsoft.Xna.Framework;
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
        private const float BaseSpawnCooldown = 1.2f; // Base spawn frequency
        private const float MinSpawnCooldown = 0.3f; // Fastest possible spawn
        private const float MaxSpawnCooldown = 3.0f; // Slowest possible spawn
        private const float SpawnAdjustmentRate = 0.1f; // How quickly to adjust spawn rate
        
        private List<Enemy> _enemies;
        private Texture2D _basicEnemyTexture;
        private Texture2D _fastEnemyTexture;
        private Texture2D _tankEnemyTexture;
        private Texture2D _zigzagEnemyTexture;
        private Texture2D _kamikazeEnemyTexture;
        private Texture2D _shooterEnemyTexture;
        private float _spawnTimer;
        private float _currentSpawnCooldown;
        private Rectangle _screenBounds;
        private Random _random;
        private BulletManager _bulletManager;
        private SoundManager _soundManager;
        
        // Performance tracking
        private int _enemiesKilled;
        private int _enemiesEscaped;
        private int _totalEnemiesSpawned;

        public EnemyManager(ContentManager content, Rectangle bounds, BulletManager bulletManager, SoundManager soundManager)
        {
            _enemies = new List<Enemy>();
            _random = new Random();
            _screenBounds = bounds;
            _bulletManager = bulletManager;
            _soundManager = soundManager;
            
            // Initialize dynamic spawning
            _currentSpawnCooldown = BaseSpawnCooldown;
            _enemiesKilled = 0;
            _enemiesEscaped = 0;
            _totalEnemiesSpawned = 0;
            
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
            
            // Adjust spawn rate based on performance
            AdjustSpawnRate();
            
            _spawnTimer -= deltaTime;
            
            if (_spawnTimer <= 0)
            {
                SpawnEnemy();
                _spawnTimer = _currentSpawnCooldown;
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
                    _soundManager.PlayEnemyShoot();
                    enemy.ResetShootTimer(_random);
                }
                
                // Remove enemy if off screen (track as escaped)
                if (enemy.Position.Y > _screenBounds.Height + 50)
                {
                    _enemies.Remove(enemy);
                    _enemiesEscaped++;
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
            _totalEnemiesSpawned++;
        }

        public Enemy CheckBulletCollision(Rectangle bulletBounds)
        {
            foreach (var enemy in _enemies.ToList())
            {
                if (enemy.Bounds.Intersects(bulletBounds))
                {
                    _enemies.Remove(enemy);
                    _enemiesKilled++;
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

        private void AdjustSpawnRate()
        {
            // Only adjust if we have enough data (at least 5 enemies spawned)
            if (_totalEnemiesSpawned < 5) return;
            
            // Calculate kill rate (enemies killed vs total spawned)
            float killRate = (float)_enemiesKilled / _totalEnemiesSpawned;
            float escapeRate = (float)_enemiesEscaped / _totalEnemiesSpawned;
            
            // If player is killing many enemies, spawn faster (decrease cooldown)
            if (killRate > 0.7f) // 70% kill rate
            {
                _currentSpawnCooldown = Math.Max(MinSpawnCooldown, 
                    _currentSpawnCooldown - SpawnAdjustmentRate);
            }
            // If many enemies are escaping, spawn slower (increase cooldown)
            else if (escapeRate > 0.3f) // 30% escape rate
            {
                _currentSpawnCooldown = Math.Min(MaxSpawnCooldown, 
                    _currentSpawnCooldown + SpawnAdjustmentRate);
            }
            // Otherwise, gradually return to base rate
            else
            {
                if (_currentSpawnCooldown < BaseSpawnCooldown)
                {
                    _currentSpawnCooldown = Math.Min(BaseSpawnCooldown, 
                        _currentSpawnCooldown + SpawnAdjustmentRate * 0.5f);
                }
                else if (_currentSpawnCooldown > BaseSpawnCooldown)
                {
                    _currentSpawnCooldown = Math.Max(BaseSpawnCooldown, 
                        _currentSpawnCooldown - SpawnAdjustmentRate * 0.5f);
                }
            }
        }

        public List<Enemy> GetActiveEnemies()
        {
            return _enemies; // All enemies in the list are active (inactive ones are removed)
        }

        public void Reset()
        {
            _enemies.Clear();
            _spawnTimer = 0;
            _currentSpawnCooldown = BaseSpawnCooldown;
            _enemiesKilled = 0;
            _enemiesEscaped = 0;
            _totalEnemiesSpawned = 0;
        }
    }
}

