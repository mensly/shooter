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
        private InputManager _inputManager;
        private UIManager _uiManager;
        private GameStateManager _gameStateManager;
        private RespawnManager _respawnManager;
        private Texture2D _starTexture;
        
        // Fixed resolution for gameplay
        private const int GameWidth = 1920;
        private const int GameHeight = 1080;
        private const int PlayerSpawnY = 130; // Distance from bottom of screen
        private const int InitialLives = 3;
        
        private RenderTarget2D _renderTarget;
        private Rectangle _gameBounds;
        private bool _fullscreen;
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
            
            // Initialize managers
            _inputManager = new InputManager();
            _uiManager = new UIManager(_inputManager);
            _uiManager.LoadContent(Content);
            
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
            _respawnManager = new RespawnManager(_playerSpawnPosition, _player, _enemyManager, _bulletManager);
            
            _gameStateManager = new GameStateManager(_player, _enemyManager, _bulletManager, 
                _collisionManager, _respawnManager, _inputManager);
            
            _collisionManager.OnEnemyHit += _gameStateManager.OnEnemyHit;
            _collisionManager.OnPlayerHit += _gameStateManager.OnPlayerHit;
            
            _fullscreen = false;
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(PlayerIndex.One);
            
            // Update input manager
            _inputManager.Update(gameTime);
            
            // Handle input
            if (_inputManager.IsFullscreenTogglePressed(keyboardState))
            {
                ToggleFullscreen();
            }
            
            if (_inputManager.IsExitPressed(keyboardState, gamePadState))
            {
                Exit();
            }
            
            if (!_gameStateManager.GameOver)
            {
                _backgroundManager.Update(gameTime);
                _player.Update(gameTime, keyboardState, gamePadState);
                _enemyManager.Update(gameTime);
                _bulletManager.Update(gameTime);
                
                // Check for shooting
                if (_inputManager.IsShooting(keyboardState, gamePadState))
                {
                    _bulletManager.AddPlayerBullet(_player.GetBulletSpawnPosition());
                    _soundManager.PlayShoot();
                }
                
                // Update game state
                _gameStateManager.Update(gameTime);
            }
            else
            {
                // Check for restart
                if (_inputManager.IsRestartPressed(keyboardState, gamePadState))
                {
                    _gameStateManager.RestartGame();
                }
            }
            
            // Update input states at the end of the frame
            _inputManager.EndFrame();

            base.Update(gameTime);
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
            
            if (!_gameStateManager.GameOver)
            {
                _player.Draw(_spriteBatch);
                _enemyManager.Draw(_spriteBatch);
                _bulletManager.Draw(_spriteBatch);
                
                // Draw game UI
                _uiManager.DrawGameUI(_spriteBatch, _gameStateManager.Score, _gameStateManager.Lives);
            }
            else
            {
                // Draw game over UI
                _uiManager.DrawGameOverUI(_spriteBatch, _gameStateManager.Score);
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

