using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
        private SoundManager _soundManager;
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
        
        // Input detection for adaptive UI
        private bool _usingKeyboard;
        private bool _usingGamepad;
        private float _keyboardInputTime;
        private float _gamepadInputTime;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Set larger window size (maintaining 16:9 aspect ratio)
            _graphics.PreferredBackBufferWidth = GameWidth;
            _graphics.PreferredBackBufferHeight = GameHeight;
            
#if !DEBUG
            // Start in fullscreen for release builds
            _graphics.IsFullScreen = true;
            _fullscreen = true;
            _graphics.HardwareModeSwitch = true;
#endif
            
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
            
            // Initialize sound manager
            _soundManager = new SoundManager();
            _soundManager.LoadContent(Content);
            
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
            _enemyManager = new EnemyManager(Content, _gameBounds, _bulletManager, _soundManager);
            
            _collisionManager = new CollisionManager(_bulletManager, _enemyManager, _player, _soundManager);
            _collisionManager.OnEnemyHit += OnEnemyHit;
            _collisionManager.OnPlayerHit += OnPlayerHit;
            
            _score = 0;
            _lives = InitialLives;
            _gameOver = false;
            
            _previousKeyboardState = Keyboard.GetState();
            _previousGamePadState = GamePad.GetState(PlayerIndex.One);
            _fullscreen = false;
            
            // Initialize input detection
            _usingKeyboard = false;
            _usingGamepad = false;
            _keyboardInputTime = 0f;
            _gamepadInputTime = 0f;
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            
            // Handle F11 fullscreen toggle
            if (keyboardState.IsKeyDown(Keys.F11) && !_previousKeyboardState.IsKeyDown(Keys.F11))
            {
                ToggleFullscreen();
            }
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            var gamePadState = GamePad.GetState(PlayerIndex.One);
            
            if (!_gameOver)
            {
                _backgroundManager.Update(gameTime);
                _player.Update(gameTime, keyboardState, gamePadState);
                _enemyManager.Update(gameTime);
                _bulletManager.Update(gameTime);
                
                // Track input usage for adaptive UI
                TrackInputUsage(keyboardState, gamePadState, gameTime);
                
                // Check for shooting (only on button press, not while held)
                if (IsShooting(keyboardState, gamePadState, _previousKeyboardState, _previousGamePadState))
                {
                    _bulletManager.AddPlayerBullet(_player.GetBulletSpawnPosition());
                    _soundManager.PlayShoot();
                }
                
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
                // Check for restart (keyboard or gamepad)
                if (keyboardState.IsKeyDown(Keys.R) || 
                    (gamePadState.IsButtonDown(Buttons.Start) && !_previousGamePadState.IsButtonDown(Buttons.Start)))
                {
                    RestartGame();
                }
            }
            
            // Update previous states at the end of the frame
            _previousKeyboardState = keyboardState;
            _previousGamePadState = gamePadState;

            base.Update(gameTime);
        }

        private void TrackInputUsage(KeyboardState keyboardState, GamePadState gamePadState, GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Check for keyboard input (movement or shooting)
            bool keyboardInput = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.Right) || 
                                keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.D) ||
                                keyboardState.IsKeyDown(Keys.Space);
            
            // Check for gamepad input (movement or shooting)
            bool gamepadInput = gamePadState.ThumbSticks.Left.X != 0 || gamePadState.ThumbSticks.Left.Y != 0 ||
                               gamePadState.DPad.Left == ButtonState.Pressed || gamePadState.DPad.Right == ButtonState.Pressed ||
                               gamePadState.IsButtonDown(Buttons.A) || gamePadState.Triggers.Left > 0.1f || 
                               gamePadState.Triggers.Right > 0.1f;
            
            // Track input time
            if (keyboardInput)
            {
                _keyboardInputTime += deltaTime;
                _usingKeyboard = true;
            }
            
            if (gamepadInput)
            {
                _gamepadInputTime += deltaTime;
                _usingGamepad = true;
            }
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
        
        private void RestartGame()
        {
            _score = 0;
            _lives = InitialLives;
            _gameOver = false;
            ResetPlayerPosition();
            _enemyManager.Reset();
            _bulletManager.Reset();
            
            // Reset input detection for new game
            _usingKeyboard = false;
            _usingGamepad = false;
            _keyboardInputTime = 0f;
            _gamepadInputTime = 0f;
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
                _graphics.HardwareModeSwitch = true;
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
            if (!_gameOver)
            {
                // Score with large font (during gameplay)
                _spriteBatch.DrawString(_fontLarge, _score.ToString(), new Vector2(20, 20), Color.White);
                
                // Lives with heart graphics
                var heartSize = 40;
                var livesY = 100; // Increased gap between score and lives
                for (int i = 0; i < _lives; i++)
                {
                    _spriteBatch.Draw(_heartTexture, 
                        new Rectangle(20 + i * (heartSize + 10), livesY, heartSize, heartSize), 
                        Color.White);
                }
            }
            else
            {
                // Game over screen with centered score
                var gameOverText = "GAME OVER";
                var scoreText = $"Final Score: {_score}";
                
                // Adaptive restart message based on input detection
                string restartText;
                if (_usingGamepad && !_usingKeyboard)
                {
                    restartText = "Press Start to Restart";
                }
                else if (_usingKeyboard && !_usingGamepad)
                {
                    restartText = "Press R to Restart";
                }
                else
                {
                    restartText = "Press R or Start to Restart";
                }
                
                var gameOverSize = _font.MeasureString(gameOverText);
                var scoreSize = _fontLarge.MeasureString(scoreText);
                var restartSize = _font.MeasureString(restartText);
                
                // Game Over text
                _spriteBatch.DrawString(_font, gameOverText, 
                    new Vector2(GameWidth / 2 - gameOverSize.X / 2, 
                    GameHeight / 2 - 100), Color.Red);
                
                // Final Score with large font, centered
                _spriteBatch.DrawString(_fontLarge, scoreText, 
                    new Vector2(GameWidth / 2 - scoreSize.X / 2, 
                    GameHeight / 2 - 20), Color.White);
                
                // Restart text
                _spriteBatch.DrawString(_font, restartText, 
                    new Vector2(GameWidth / 2 - restartSize.X / 2, 
                    GameHeight / 2 + 80), Color.Yellow);
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

