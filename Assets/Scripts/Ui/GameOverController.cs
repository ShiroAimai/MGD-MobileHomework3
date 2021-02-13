using UnityEngine;
using UnityEngine.UI;
using Utils;
using Image = UnityEngine.UI.Image;

namespace Ui
{
    public class GameOverController : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text scoreText;
        [SerializeField] private Image scoreBackground;
        [SerializeField] private Image highScoreBackground;
        
        public void GameOver(int gameScore)
        {
            scoreText.text = gameScore.ToString();
            TryToSetNewHighScore(gameScore);
            gameOverPanel.SetActive(true);
        }

        private void TryToSetNewHighScore(int gameScore)
        {
            int highScore = PlayerPrefs.GetInt(PlayerPrefHelper.HighscoreKey, 0);

            bool isNewHighScore = gameScore > highScore;
            scoreBackground.enabled = !isNewHighScore; 
            highScoreBackground.enabled = isNewHighScore;
            
            if (!isNewHighScore) return;
            PlayerPrefs.SetInt(PlayerPrefHelper.HighscoreKey, gameScore);
        }
    }
}