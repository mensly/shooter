using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Shooter.Managers
{
    public class BackgroundManager
    {
        private Rectangle _screenBounds;
        private Color _starColor;
        private Vector2[] _stars;
        private float[] _starSpeeds;
        private Random _random;

        public BackgroundManager(Rectangle screenBounds)
        {
            _screenBounds = screenBounds;
            _random = new Random();
            _starColor = Color.White;
            
            InitializeStars();
        }

        private void InitializeStars()
        {
            _stars = new Vector2[50];
            _starSpeeds = new float[50];
            
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new Vector2(
                    _random.Next(0, _screenBounds.Width),
                    _random.Next(0, _screenBounds.Height)
                );
                // Vary star speeds for parallax effect
                _starSpeeds[i] = _random.Next(50, 200);
            }
        }

        public void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            for (int i = 0; i < _stars.Length; i++)
            {
                // Move star down
                _stars[i] = new Vector2(
                    _stars[i].X,
                    _stars[i].Y + _starSpeeds[i] * deltaTime
                );
                
                // Reset star to top when it goes off screen
                if (_stars[i].Y > _screenBounds.Height)
                {
                    _stars[i] = new Vector2(
                        _random.Next(0, _screenBounds.Width),
                        -10
                    );
                }
            }
        }

        public void DrawStars(SpriteBatch spriteBatch, Texture2D starTexture)
        {
            foreach (var star in _stars)
            {
                // Draw brighter stars (3x3 pixels)
                spriteBatch.Draw(starTexture, new Rectangle((int)star.X, (int)star.Y, 3, 3), Color.White);
            }
        }
    }
}

