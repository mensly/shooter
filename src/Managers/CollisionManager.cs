using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Shooter.Entities;
using System;

namespace Shooter.Managers
{
    public class CollisionManager
    {
        private readonly BulletManager _bulletManager;
        private readonly EnemyManager _enemyManager;
        private readonly Player _player;
        private readonly SoundEffect _explosionSound;
        private readonly SoundEffect _hitSound;
        
        // Events for collision handling
        public event Action<int> OnEnemyHit; // Passes score value
        public event Action OnPlayerHit;

        public CollisionManager(BulletManager bulletManager, EnemyManager enemyManager, Player player,
            SoundEffect explosionSound, SoundEffect hitSound)
        {
            _bulletManager = bulletManager;
            _enemyManager = enemyManager;
            _player = player;
            _explosionSound = explosionSound;
            _hitSound = hitSound;
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
                    _explosionSound.Play();
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
                    _hitSound.Play();
                    OnPlayerHit?.Invoke();
                }
            }
        }
    }
}

