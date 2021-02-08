using UnityEngine;
using UnityEngine.UI;
using Utils;

public class HighScoreController : MonoBehaviour
{
    [SerializeField] private Text highScoreText;
    private void Start()
    {
        string playerHighScore = PlayerPrefs.GetString(PlayerPrefHelper.HighscoreKey, null);
        highScoreText.text = string.IsNullOrEmpty(playerHighScore) ? "0" : playerHighScore;
    }
}
