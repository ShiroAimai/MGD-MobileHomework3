using System;
using Managers;
using UnityEngine;

namespace Ui
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private String gameSceneName = "GameScene";

        public void StartGame()
        {
            SceneTransitionManager.instance.LoadScene(gameSceneName);
        }

        public void Quit()
        {
            SceneTransitionManager.instance.ExitGame();
        }
    }
}