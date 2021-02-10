using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui
{
    public class HighScoreController : MonoBehaviour
    {
        [SerializeField] private Text highScoreText;
        private void Start()
        {
            int playerHighScore = PlayerPrefs.GetInt(PlayerPrefHelper.HighscoreKey, 0);
            highScoreText.text = playerHighScore.ToString();
        }
    }
}
