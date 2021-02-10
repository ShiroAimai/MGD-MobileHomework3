using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject highScoreLabel;
        [SerializeField] private Text scoreText;
        [SerializeField] private Color normalScoreColor;   
        [SerializeField] private Color highScoreColor;   
        
        public void GameOver(int gameScore)
        {
            scoreText.text = gameScore.ToString();
            TryToSetNewHighScore(gameScore);
            gameOverPanel.SetActive(true);
        }

        private void TryToSetNewHighScore(int gameScore)
        {
            int highScore = PlayerPrefs.GetInt(PlayerPrefHelper.HighscoreKey, 0);
            scoreText.color = gameScore <= highScore ? normalScoreColor : highScoreColor;
            if (gameScore <= highScore) return;
            highScoreLabel.SetActive(true);
            
            PlayerPrefs.SetInt(PlayerPrefHelper.HighscoreKey, gameScore);
        }
    }
}