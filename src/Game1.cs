using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shooter.Entities;
using Shooter.Managers;
using System.Collections.Generic;

namespace Shooter
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        
        private Player _player;
        private EnemyManager _enemyManager;
        private BulletManager _bulletManager;
        private BackgroundManager _backgroundManager;
        private Texture2D _starTexture;
        
        private SpriteFont _font;
        private int _score;
        private int _lives;
        private bool _gameOver;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            _font = Content.Load<SpriteFont>("font");
            
            _backgroundManager = new BackgroundManager(GraphicsDevice.Viewport.Bounds);
            
            // Create a simple star texture
            _starTexture = new Texture2D(GraphicsDevice, 1, 1);
            _starTexture.SetData(new[] { Color.White });
            
            _player = new Player(Content.Load<Texture2D>("player"))
            {
                Position = new Vector2(512, 650)
            };
            
            _enemyManager = new EnemyManager(Content);
            _bulletManager = new BulletManager();
            _bulletManager.LoadContent(Content);
            
            _score = 0;
            _lives = 3;
            _gameOver = false;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (!_gameOver)
            {
                var keyboardState = Keyboard.GetState();
                
                _backgroundManager.Update(gameTime);
                _player.Update(gameTime, keyboardState);
                _enemyManager.Update(gameTime);
                _bulletManager.Update(gameTime);
                
                // Check for shooting
                if (keyboardState.IsKeyDown(Keys.Space))
                {
                    _bulletManager.AddPlayerBullet(_player.GetBulletSpawnPosition());
                }
                
                // Collision detection
                CheckCollisions();
                
                // Check if player is hit
                if (_enemyManager.CheckPlayerCollision(_player.Bounds))
                {
                    _lives--;
                    if (_lives <= 0)
                    {
                        _gameOver = true;
                    }
                    else
                    {
                        _player.Position = new Vector2(512, 650);
                    }
                }
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    _score = 0;
                    _lives = 3;
                    _gameOver = false;
                    _player.Position = new Vector2(512, 650);
                    _enemyManager.Reset();
                    _bulletManager.Reset();
                }
            }

            base.Update(gameTime);
        }

        private void CheckCollisions()
        {
            // Player bullets hitting enemies
            foreach (var bullet in _bulletManager.PlayerBullets)
            {
                var hitEnemy = _enemyManager.CheckBulletCollision(bullet.Bounds);
                if (hitEnemy != null)
                {
                    _score += hitEnemy.ScoreValue;
                    bullet.IsActive = false;
                }
            }
            
            // Enemy bullets hitting player
            foreach (var bullet in _bulletManager.EnemyBullets)
            {
                if (bullet.Bounds.Intersects(_player.Bounds))
                {
                    _lives--;
                    bullet.IsActive = false;
                    if (_lives <= 0)
                    {
                        _gameOver = true;
                    }
                    else
                    {
                        _player.Position = new Vector2(512, 650);
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            
            _backgroundManager.DrawStars(_spriteBatch, _starTexture);
            
            if (!_gameOver)
            {
                _player.Draw(_spriteBatch);
                _enemyManager.Draw(_spriteBatch);
                _bulletManager.Draw(_spriteBatch);
            }
            
            // UI
            _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, $"Lives: {_lives}", new Vector2(10, 40), Color.White);
            
            if (_gameOver)
            {
                var gameOverText = "GAME OVER";
                var restartText = "Press R to Restart";
                var textSize = _font.MeasureString(gameOverText);
                var restartSize = _font.MeasureString(restartText);
                
                _spriteBatch.DrawString(_font, gameOverText, 
                    new Vector2(GraphicsDevice.Viewport.Width / 2 - textSize.X / 2, 
                    GraphicsDevice.Viewport.Height / 2 - 50), Color.Red);
                _spriteBatch.DrawString(_font, restartText, 
                    new Vector2(GraphicsDevice.Viewport.Width / 2 - restartSize.X / 2, 
                    GraphicsDevice.Viewport.Height / 2 + 20), Color.White);
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

