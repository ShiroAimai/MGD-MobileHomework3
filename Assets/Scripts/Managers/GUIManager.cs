
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
	public class GUIManager : MonoBehaviour {
		public static GUIManager instance;

		public GameObject gameOverPanel;
		public Text yourScoreTxt;
		public Text highScoreTxt;

		public Text scoreTxt;
		public Text moveCounterTxt;

		private int score;

		public int Score
		{
			get => score;
			set
			{
				score = value;
				scoreTxt.text = score.ToString();
			}
		}
		private int moveCounter;
		public int MoveCounter
		{
			get => moveCounter;
			set {
				moveCounter = value;
				if (moveCounter <= 0)
				{
					moveCounter = 0;
					StartCoroutine(WaitForShiftingDone());
				}
				moveCounterTxt.text = moveCounter.ToString();
			}
		}
		void Awake()
		{
			MoveCounter = 2;
		
			instance = GetComponent<GUIManager>();
		}

		// Show the game over panel
		public void GameOver() {
			SceneTransitionManager.instance.gameOver = true;

			gameOverPanel.SetActive(true);

			if (score > PlayerPrefs.GetInt("HighScore")) {
				PlayerPrefs.SetInt("HighScore", score);
				highScoreTxt.text = "New Best: " + PlayerPrefs.GetInt("HighScore").ToString();
			} else {
				highScoreTxt.text = "Best: " + PlayerPrefs.GetInt("HighScore").ToString();
			}

			yourScoreTxt.text = score.ToString();
		}
	
		private IEnumerator WaitForShiftingDone() {
			yield return new WaitUntil(() => !BoardManager.instance.IsBoardBusy);
			yield return new WaitForSeconds(.25f);
			GameOver();
		}


	}
}
