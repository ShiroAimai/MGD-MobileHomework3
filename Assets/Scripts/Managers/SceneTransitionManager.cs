using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
	public class SceneTransitionManager : MonoBehaviour {
		public static SceneTransitionManager instance;

		[SerializeField] private GameObject faderObj;
		[SerializeField] private Image faderImg;
		public bool gameOver = false;

		private bool isReturning = false;

		// Get the current scene name
		private string currentScene;
		public string CurrentSceneName => currentScene;
		
		[SerializeField] private float fadeSpeed = .02f;

		private Color fadeTransparency = new Color(0, 0, 0, .04f);
		private AsyncOperation async;

		void Awake() {
			// Only 1 Game Manager can exist at a time
			if (instance == null) {
				DontDestroyOnLoad(gameObject);
				instance = this;
				SceneManager.sceneLoaded += OnLevelFinishedLoading;
			} else {
				Destroy(gameObject);
			}
		}
		

		// Load a scene with a specified string name
		public void LoadScene(string sceneName) {
			instance.StartCoroutine(Load(sceneName));
			instance.StartCoroutine(FadeOut(instance.faderObj, instance.faderImg));
		}

		// Reload the current scene
		public void ReloadScene() {
			LoadScene(SceneManager.GetActiveScene().name);
		}

		private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
			currentScene = scene.name;
			instance.StartCoroutine(FadeIn(instance.faderObj, instance.faderImg));
		}

		//Iterate the fader transparency to 100%
		IEnumerator FadeOut(GameObject faderObject, Image fader) {
			faderObject.SetActive(true);
			while (fader.color.a < 1) {
				fader.color += fadeTransparency;
				yield return new WaitForSeconds(fadeSpeed);
			}
			ActivateScene(); //Activate the scene when the fade ends
		}

		// Iterate the fader transparency to 0%
		IEnumerator FadeIn(GameObject faderObject, Image fader) {
			while (fader.color.a > 0) {
				fader.color -= fadeTransparency;
				yield return new WaitForSeconds(fadeSpeed);
			}
			faderObject.SetActive(false);
		}

		// Begin loading a scene with a specified string asynchronously
		IEnumerator Load(string sceneName) {
			async = SceneManager.LoadSceneAsync(sceneName);
			async.allowSceneActivation = false;
			yield return async;
			isReturning = false;
		}

		// Allows the scene to change once it is loaded
		public void ActivateScene() {
			async.allowSceneActivation = true;
		}
		

		public void ExitGame() {
			// If we are running in a standalone build of the game
#if UNITY_STANDALONE
			// Quit the application
			Application.Quit();
#endif

			// If we are running in the editor
#if UNITY_EDITOR
			// Stop playing the scene
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}

		public void ReturnToMenu() {
			if (isReturning) {
				return;
			}

			if (CurrentSceneName == "MainScene") return;
			StopAllCoroutines();
			LoadScene("MainScene");
			isReturning = true;
		}

	}
}
