using Microsoft.Xna.Framework;
using Shooter.Entities;
using System;

namespace Shooter.Managers
{
    public class CollisionManager
    {
        private readonly BulletManager _bulletManager;
        private readonly EnemyManager _enemyManager;
        private readonly Player _player;
        private readonly SoundManager _soundManager;
        
        // Events for collision handling
        public event Action<int> OnEnemyHit; // Passes score value
        public event Action OnPlayerHit;

        public CollisionManager(BulletManager bulletManager, EnemyManager enemyManager, Player player,
            SoundManager soundManager)
        {
            _bulletManager = bulletManager;
            _enemyManager = enemyManager;
            _player = player;
            _soundManager = soundManager;
        }

        public void Update()
        {
            CheckPlayerBulletCollisions();
            CheckEnemyBulletCollisions();
        }

        private void CheckPlayerBulletCollisions()
        {
            // Player bullets hitting enemies
            foreach (var bullet in _bulletManager.PlayerBullets)
            {
                var hitEnemy = _enemyManager.CheckBulletCollision(bullet.Bounds);
                if (hitEnemy != null)
                {
                    bullet.IsActive = false;
                    _soundManager.PlayExplosion();
                    OnEnemyHit?.Invoke(hitEnemy.ScoreValue);
                }
            }
        }

        private void CheckEnemyBulletCollisions()
        {
            // Enemy bullets hitting player
            foreach (var bullet in _bulletManager.EnemyBullets)
            {
                if (bullet.Bounds.Intersects(_player.Bounds))
                {
                    bullet.IsActive = false;
                    _soundManager.PlayHit();
                    OnPlayerHit?.Invoke();
                }
            }
        }
    }
}
