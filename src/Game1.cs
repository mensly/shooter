using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shooter.Entities;
using Shooter.Managers;
using System;
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
        
        // Fixed resolution for gameplay
        private const int GameWidth = 1920;
        private const int GameHeight = 1080;
        private RenderTarget2D _renderTarget;
        private Rectangle _gameBounds;
        private bool _fullscreen;
        private KeyboardState _previousKeyboardState;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Set larger window size (maintaining 16:9 aspect ratio)
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create render target for fixed resolution gameplay
            _renderTarget = new RenderTarget2D(GraphicsDevice, GameWidth, GameHeight);
            _gameBounds = new Rectangle(0, 0, GameWidth, GameHeight);
            
            _font = Content.Load<SpriteFont>("font");
            
            _backgroundManager = new BackgroundManager(_gameBounds);
            
            // Create a simple star texture
            _starTexture = new Texture2D(GraphicsDevice, 1, 1);
            _starTexture.SetData(new[] { Color.White });
            
            _player = new Player(Content.Load<Texture2D>("player"), _gameBounds)
            {
                Position = new Vector2(GameWidth / 2, GameHeight - 130)
            };
            
            _enemyManager = new EnemyManager(Content, _gameBounds);
            _bulletManager = new BulletManager(_gameBounds);
            _bulletManager.LoadContent(Content);
            
            _score = 0;
            _lives = 3;
            _gameOver = false;
            
            _previousKeyboardState = SubscribeToKeyboard();
            _fullscreen = false;
        }
        
        private KeyboardState SubscribeToKeyboard()
        {
            return Keyboard.GetState();
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            
            // Handle F11 fullscreen toggle
            if (keyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11))
            {
                ToggleFullscreen();
            }
            _previousKeyboardState = keyboardState;
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (!_gameOver)
            {
                var gamePadState = GamePad.GetState(PlayerIndex.One);
                
                _backgroundManager.Update(gameTime);
                _player.Update(gameTime, keyboardState, gamePadState);
                _enemyManager.Update(gameTime);
                _bulletManager.Update(gameTime);
                
                // Check for shooting (keyboard or gamepad)
                if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.IsButtonDown(Buttons.A))
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
                        _player.Position = new Vector2(GameWidth / 2, GameHeight - 130);
                    }
                }
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.R))
                {
                    _score = 0;
                    _lives = 3;
                    _gameOver = false;
                    _player.Position = new Vector2(GameWidth / 2, GameHeight - 130);
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

        private void ToggleFullscreen()
        {
            _fullscreen = !_fullscreen;
            
            if (_fullscreen)
            {
                // Get display resolution
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.IsFullScreen = true;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = 1280;
                _graphics.PreferredBackBufferHeight = 720;
                _graphics.IsFullScreen = false;
            }
            
            _graphics.ApplyChanges();
        }
        
        protected override void Draw(GameTime gameTime)
        {
            // Set render target to our fixed resolution buffer
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            
            _backgroundManager.DrawStars(_spriteBatch, _starTexture);
            
            if (!_gameOver)
            {
                _player.Draw(_spriteBatch);
                _enemyManager.Draw(_spriteBatch);
                _bulletManager.Draw(_spriteBatch);
            }
            
            // UI (render to game resolution)
            _spriteBatch.DrawString(_font, $"Score: {_score}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(_font, $"Lives: {_lives}", new Vector2(10, 40), Color.White);
            
            if (_gameOver)
            {
                var gameOverText = "GAME OVER";
                var restartText = "Press R to Restart";
                var textSize = _font.MeasureString(gameOverText);
                var restartSize = _font.MeasureString(restartText);
                
                _spriteBatch.DrawString(_font, gameOverText, 
                    new Vector2(GameWidth / 2 - textSize.X / 2, 
                    GameHeight / 2 - 50), Color.Red);
                _spriteBatch.DrawString(_font, restartText, 
                    new Vector2(GameWidth / 2 - restartSize.X / 2, 
                    GameHeight / 2 + 20), Color.White);
            }
            
            _spriteBatch.End();
            
            // Now draw the render target to the screen, scaled to fit
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            
            // Calculate the scale to fit the screen while maintaining aspect ratio
            float scaleX = (float)GraphicsDevice.Viewport.Width / GameWidth;
            float scaleY = (float)GraphicsDevice.Viewport.Height / GameHeight;
            float scale = Math.Min(scaleX, scaleY);
            
            // Calculate centered position
            int scaledWidth = (int)(GameWidth * scale);
            int scaledHeight = (int)(GameHeight * scale);
            int offsetX = (GraphicsDevice.Viewport.Width - scaledWidth) / 2;
            int offsetY = (GraphicsDevice.Viewport.Height - scaledHeight) / 2;
            
            Rectangle destinationRect = new Rectangle(offsetX, offsetY, scaledWidth, scaledHeight);
            _spriteBatch.Draw(_renderTarget, destinationRect, Color.White);
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

