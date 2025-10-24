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
        private CollisionManager _collisionManager;
        private Texture2D _starTexture;
        private Texture2D _heartTexture;
        
        private SpriteFont _font;
        private SpriteFont _fontLarge;
        private int _score;
        private int _lives;
        private bool _gameOver;
        
        // Fixed resolution for gameplay
        private const int GameWidth = 1920;
        private const int GameHeight = 1080;
        private const int PlayerSpawnY = 130; // Distance from bottom of screen
        private const int InitialLives = 3;
        
        private RenderTarget2D _renderTarget;
        private Rectangle _gameBounds;
        private bool _fullscreen;
        private KeyboardState _previousKeyboardState;
        private GamePadState _previousGamePadState;
        private Vector2 _playerSpawnPosition;

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
            _playerSpawnPosition = new Vector2(GameWidth / 2, GameHeight - PlayerSpawnY);
            
            _font = Content.Load<SpriteFont>("font");
            _fontLarge = Content.Load<SpriteFont>("font_large");
            _heartTexture = Content.Load<Texture2D>("heart");
            
            _backgroundManager = new BackgroundManager(_gameBounds);
            
            // Create a simple star texture
            _starTexture = new Texture2D(GraphicsDevice, 1, 1);
            _starTexture.SetData(new[] { Color.White });
            
            _player = new Player(Content.Load<Texture2D>("player"), _gameBounds)
            {
                Position = _playerSpawnPosition
            };
            
            _bulletManager = new BulletManager(_gameBounds);
            _bulletManager.LoadContent(Content);
            _enemyManager = new EnemyManager(Content, _gameBounds, _bulletManager);
            
            _collisionManager = new CollisionManager(_bulletManager, _enemyManager, _player);
            _collisionManager.OnEnemyHit += OnEnemyHit;
            _collisionManager.OnPlayerHit += OnPlayerHit;
            
            _score = 0;
            _lives = InitialLives;
            _gameOver = false;
            
            _previousKeyboardState = Keyboard.GetState();
            _previousGamePadState = GamePad.GetState(PlayerIndex.One);
            _fullscreen = false;
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
                
                // Check for shooting (only on button press, not while held)
                if (IsShooting(keyboardState, gamePadState, _previousKeyboardState, _previousGamePadState))
                {
                    _bulletManager.AddPlayerBullet(_player.GetBulletSpawnPosition());
                }
                
                _previousGamePadState = gamePadState;
                
                // Collision detection
                _collisionManager.Update();
                
                // Check if player hits enemy (enemy collision with player)
                if (_enemyManager.CheckPlayerCollision(_player.Bounds))
                {
                    OnPlayerHit();
                }
            }
            else
            {
                if (keyboardState.IsKeyDown(Keys.R))
                {
                    RestartGame();
                }
            }

            base.Update(gameTime);
        }

        private bool IsShooting(KeyboardState keyboardState, GamePadState gamePadState, 
            KeyboardState previousKeyboardState, GamePadState previousGamePadState)
        {
            // Only shoot when button transitions from not pressed to pressed
            bool spacePressed = keyboardState.IsKeyDown(Keys.Space) && !previousKeyboardState.IsKeyDown(Keys.Space);
            bool buttonAPressed = gamePadState.IsButtonDown(Buttons.A) && !previousGamePadState.IsButtonDown(Buttons.A);
            
            // Trigger buttons (check if pressed beyond threshold)
            bool leftTriggerPressed = gamePadState.Triggers.Left > 0.5f && previousGamePadState.Triggers.Left <= 0.5f;
            bool rightTriggerPressed = gamePadState.Triggers.Right > 0.5f && previousGamePadState.Triggers.Right <= 0.5f;
            
            return spacePressed || buttonAPressed || leftTriggerPressed || rightTriggerPressed;
        }
        
        private void OnEnemyHit(int scoreValue)
        {
            _score += scoreValue;
        }
        
        private void OnPlayerHit()
        {
            _lives--;
            if (_lives <= 0)
            {
                _gameOver = true;
            }
            else
            {
                ResetPlayerPosition();
            }
        }
        
        private void ResetPlayerPosition()
        {
            _player.Position = _playerSpawnPosition;
        }
        
        private void RestartGame()
        {
            _score = 0;
            _lives = InitialLives;
            _gameOver = false;
            ResetPlayerPosition();
            _enemyManager.Reset();
            _bulletManager.Reset();
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
            // Score with large font
            _spriteBatch.DrawString(_fontLarge, $"Score: {_score}", new Vector2(20, 20), Color.White);
            
            // Lives with heart graphics
            var heartSize = 40;
            var livesY = 100; // Increased gap between score and lives
            for (int i = 0; i < _lives; i++)
            {
                _spriteBatch.Draw(_heartTexture, 
                    new Rectangle(20 + i * (heartSize + 10), livesY, heartSize, heartSize), 
                    Color.White);
            }
            
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

