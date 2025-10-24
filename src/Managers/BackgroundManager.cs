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
            _stars = new Vector2[100];
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i] = new Vector2(
                    _random.Next(0, _screenBounds.Width),
                    _random.Next(0, _screenBounds.Height)
                );
            }
        }

        public void DrawStars(SpriteBatch spriteBatch, Texture2D starTexture)
        {
            foreach (var star in _stars)
            {
                spriteBatch.Draw(starTexture, star, Color.White);
            }
        }
    }
}

