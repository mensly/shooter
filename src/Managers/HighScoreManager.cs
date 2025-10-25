using System;
using System.IO;

namespace Shooter.Managers
{
    public class HighScoreManager
    {
        private const string HighScoreFileName = "highscore.txt";
        private int _currentHighScore;
        
        public int CurrentHighScore => _currentHighScore;
        
        public HighScoreManager()
        {
            LoadHighScore();
        }
        
        public void CheckAndUpdateHighScore(int score)
        {
            if (score > _currentHighScore)
            {
                _currentHighScore = score;
                SaveHighScore();
            }
        }
        
        public bool IsNewHighScore(int score)
        {
            return score > _currentHighScore;
        }
        
        private void LoadHighScore()
        {
            try
            {
                if (File.Exists(HighScoreFileName))
                {
                    string content = File.ReadAllText(HighScoreFileName);
                    if (int.TryParse(content, out int highScore))
                    {
                        _currentHighScore = highScore;
                    }
                    else
                    {
                        _currentHighScore = 0;
                    }
                }
                else
                {
                    _currentHighScore = 0;
                }
            }
            catch (Exception)
            {
                // If we can't read the file, default to 0
                _currentHighScore = 0;
            }
        }
        
        private void SaveHighScore()
        {
            try
            {
                File.WriteAllText(HighScoreFileName, _currentHighScore.ToString());
            }
            catch (Exception)
            {
                // If we can't save, silently fail
                // In a real game, you might want to show an error message
            }
        }
        
        public void ResetHighScore()
        {
            _currentHighScore = 0;
            SaveHighScore();
        }
    }
}
