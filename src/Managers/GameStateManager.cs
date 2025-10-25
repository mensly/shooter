using Microsoft.Xna.Framework;
using Shooter.Entities;
using Shooter.Managers;

namespace Shooter.Managers
{
    public class GameStateManager
    {
        private const int InitialLives = 3;
        
        private int _score;
        private int _lives;
        private bool _gameOver;
        
        private Player _player;
        private EnemyManager _enemyManager;
        private BulletManager _bulletManager;
        private CollisionManager _collisionManager;
        private RespawnManager _respawnManager;
        private InputManager _inputManager;
        
        public int Score => _score;
        public int Lives => _lives;
        public bool GameOver => _gameOver;
        
        public GameStateManager(Player player, EnemyManager enemyManager, BulletManager bulletManager, 
            CollisionManager collisionManager, RespawnManager respawnManager, InputManager inputManager)
        {
            _player = player;
            _enemyManager = enemyManager;
            _bulletManager = bulletManager;
            _collisionManager = collisionManager;
            _respawnManager = respawnManager;
            _inputManager = inputManager;
            
            Initialize();
        }
        
        public void Initialize()
        {
            _score = 0;
            _lives = InitialLives;
            _gameOver = false;
        }
        
        public void Update(GameTime gameTime)
        {
            if (!_gameOver)
            {
                // Collision detection
                _collisionManager.Update();
                
                // Check if player hits enemy (enemy collision with player)
                if (_enemyManager.CheckPlayerCollision(_player.Bounds))
                {
                    OnPlayerHit();
                }
            }
        }
        
        public void OnEnemyHit(int scoreValue)
        {
            _score += scoreValue;
        }
        
        public void OnPlayerHit()
        {
            _lives--;
            if (_lives <= 0)
            {
                _gameOver = true;
            }
            else
            {
                _respawnManager.ResetPlayerPosition();
            }
        }
        
        public void RestartGame()
        {
            _score = 0;
            _lives = InitialLives;
            _gameOver = false;
            _respawnManager.ResetPlayerPosition();
            _enemyManager.Reset();
            _bulletManager.Reset();
            _inputManager.Reset();
        }
    }
}
