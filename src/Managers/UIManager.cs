using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shooter.Managers;

namespace Shooter.Managers
{
    public class UIManager
    {
        private SpriteFont _font;
        private SpriteFont _fontLarge;
        private Texture2D _heartTexture;
        private InputManager _inputManager;
        
        private const int GameWidth = 1920;
        private const int GameHeight = 1080;
        
        public UIManager(InputManager inputManager)
        {
            _inputManager = inputManager;
        }
        
        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _font = content.Load<SpriteFont>("font");
            _fontLarge = content.Load<SpriteFont>("font_large");
            _heartTexture = content.Load<Texture2D>("heart");
        }
        
        public void DrawGameUI(SpriteBatch spriteBatch, int score, int lives)
        {
            // Score with large font (during gameplay)
            spriteBatch.DrawString(_fontLarge, score.ToString(), new Vector2(20, 20), Color.White);
            
            // Lives with heart graphics
            var heartSize = 40;
            var livesY = 100; // Increased gap between score and lives
            for (int i = 0; i < lives; i++)
            {
                spriteBatch.Draw(_heartTexture, 
                    new Rectangle(20 + i * (heartSize + 10), livesY, heartSize, heartSize), 
                    Color.White);
            }
        }
        
        public void DrawGameOverUI(SpriteBatch spriteBatch, int score)
        {
            var gameOverText = "GAME OVER";
            var scoreText = $"Final Score: {score}";
            
            // Adaptive restart message based on input detection
            string restartText;
            if (_inputManager.UsingGamepad && !_inputManager.UsingKeyboard)
            {
                restartText = "Press Start to Restart";
            }
            else if (_inputManager.UsingKeyboard && !_inputManager.UsingGamepad)
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
            spriteBatch.DrawString(_font, gameOverText, 
                new Vector2(GameWidth / 2 - gameOverSize.X / 2, 
                GameHeight / 2 - 100), Color.Red);
            
            // Final Score with large font, centered
            spriteBatch.DrawString(_fontLarge, scoreText, 
                new Vector2(GameWidth / 2 - scoreSize.X / 2, 
                GameHeight / 2 - 20), Color.White);
            
            // Restart text
            spriteBatch.DrawString(_font, restartText, 
                new Vector2(GameWidth / 2 - restartSize.X / 2, 
                GameHeight / 2 + 80), Color.Yellow);
        }
    }
}
