using Microsoft.Xna.Framework;
using Shooter.Entities;
using System;
using System.Collections.Generic;

namespace Shooter.Managers
{
    public class RespawnManager
    {
        private const int GameWidth = 1920;
        private const int GameHeight = 1080;
        
        private Vector2 _playerSpawnPosition;
        private Player _player;
        private EnemyManager _enemyManager;
        private BulletManager _bulletManager;
        
        public RespawnManager(Vector2 playerSpawnPosition, Player player, EnemyManager enemyManager, BulletManager bulletManager)
        {
            _playerSpawnPosition = playerSpawnPosition;
            _player = player;
            _enemyManager = enemyManager;
            _bulletManager = bulletManager;
        }
        
        public void ResetPlayerPosition()
        {
            _player.Position = FindSafeSpawnPosition();
        }
        
        private Vector2 FindSafeSpawnPosition()
        {
            const int maxAttempts = 20;
            const float spawnRadius = 100f; // How far to search from original spawn
            const float minDistanceFromEnemies = 100f; // Minimum distance from enemies
            const float minDistanceFromBullets = 100f; // Minimum distance from bullets
            
            Vector2 originalSpawn = _playerSpawnPosition;
            Random random = new Random();
            
            // Try the original spawn position first
            if (IsPositionSafe(originalSpawn, minDistanceFromEnemies, minDistanceFromBullets))
            {
                return originalSpawn;
            }
            
            // Try random positions around the original spawn
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Generate a random position within spawn radius
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = (float)(random.NextDouble() * spawnRadius);
                
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );
                
                Vector2 candidatePosition = originalSpawn + offset;
                
                // Keep position within screen bounds
                candidatePosition.X = MathHelper.Clamp(candidatePosition.X, 
                    _player.Texture.Width / 2, GameWidth - _player.Texture.Width / 2);
                candidatePosition.Y = MathHelper.Clamp(candidatePosition.Y, 
                    _player.Texture.Height / 2, GameHeight - _player.Texture.Height / 2);
                
                if (IsPositionSafe(candidatePosition, minDistanceFromEnemies, minDistanceFromBullets))
                {
                    return candidatePosition;
                }
            }
            
            // If no safe position found, return original spawn (better than nothing)
            return originalSpawn;
        }
        
        private bool IsPositionSafe(Vector2 position, float minDistanceFromEnemies, float minDistanceFromBullets)
        {
            // Check distance from enemies
            foreach (var enemy in _enemyManager.GetActiveEnemies())
            {
                float distance = Vector2.Distance(position, enemy.Position);
                if (distance < minDistanceFromEnemies)
                {
                    return false;
                }
            }
            
            // Check distance from enemy bullets
            foreach (var bullet in _bulletManager.EnemyBullets)
            {
                if (bullet.IsActive)
                {
                    float distance = Vector2.Distance(position, bullet.Position);
                    if (distance < minDistanceFromBullets)
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
    }
}
