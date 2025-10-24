using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Shooter.Managers
{
    public class BulletManager
    {
        private const int MaxPlayerBullets = 5;
        
        private List<Bullet> _playerBullets;
        private List<Bullet> _enemyBullets;
        private Texture2D _playerBulletTexture;
        private Texture2D _enemyBulletTexture;
        private readonly Rectangle _bounds;

        public IEnumerable<Bullet> PlayerBullets => _playerBullets.Where(b => b.IsActive);
        public IEnumerable<Bullet> EnemyBullets => _enemyBullets.Where(b => b.IsActive);

        public BulletManager(Rectangle bounds)
        {
            _playerBullets = new List<Bullet>();
            _enemyBullets = new List<Bullet>();
            _bounds = bounds;
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _playerBulletTexture = content.Load<Texture2D>("bullet_player");
            _enemyBulletTexture = content.Load<Texture2D>("bullet_enemy");
        }

        public void AddPlayerBullet(Vector2 position)
        {
            // Prevent too many bullets on screen
            var activeBullets = _playerBullets.Count(b => b.IsActive);
            if (activeBullets < MaxPlayerBullets)
            {
                _playerBullets.Add(new Bullet(_playerBulletTexture, position, true, _bounds));
            }
        }

        public void AddEnemyBullet(Vector2 position)
        {
            _enemyBullets.Add(new Bullet(_enemyBulletTexture, position, false, _bounds));
        }

        public void Update(GameTime gameTime)
        {
            foreach (var bullet in _playerBullets)
            {
                bullet.Update(gameTime);
            }
            
            foreach (var bullet in _enemyBullets)
            {
                bullet.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var bullet in _playerBullets.Where(b => b.IsActive))
            {
                bullet.Draw(spriteBatch);
            }
            
            foreach (var bullet in _enemyBullets.Where(b => b.IsActive))
            {
                bullet.Draw(spriteBatch);
            }
        }

        public void Reset()
        {
            _playerBullets.Clear();
            _enemyBullets.Clear();
        }
    }
}

