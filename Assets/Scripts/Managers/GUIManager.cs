using UnityEngine;

namespace Managers
{
	public class GUIManager : MonoBehaviour {
		public static GUIManager instance;

		[SerializeField] private GameObject gameOverPanel;

		#region Lifecycle
		private void Awake()
		{
			if (!instance)
				instance = this;
			else if(instance != this) Destroy(this);
		}
		#endregion

		#region Private
		
		#endregion
		// Show the game over panel
		public void GameOver() {
			SceneTransitionManager.instance.gameOver = true;

			gameOverPanel.SetActive(true);

			/*if (score > PlayerPrefs.GetInt("HighScore")) {
				PlayerPrefs.SetInt("HighScore", score);
				highScoreTxt.text = "New Best: " + PlayerPrefs.GetInt("HighScore").ToString();
			} else {
				highScoreTxt.text = "Best: " + PlayerPrefs.GetInt("HighScore").ToString();
			}*/

			//yourScoreTxt.text = score.ToString();
		}
	
		/*private IEnumerator WaitForShiftingDone() {
			yield return new WaitUntil(() => !BoardManager.instance.IsBoardBusy);
			yield return new WaitForSeconds(.25f);
			GameOver();
		}*/


	}
}
