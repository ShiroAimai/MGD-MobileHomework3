using System;
using System.Collections;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance = null;

        public bool IsGameOver { get; private set; }
        public bool IsGamePaused { get; private set; }
        
        public Action<bool> onTimeFrozen;
        public Action<int> onScoreUpdate;
        public Action<int> onComboUpdate;
        public Action<int> onGameOver;
        public Action<bool> onGamePaused;
        
        [Header("Time config")]
        [SerializeField][Tooltip("Game time. In minutes")] private int timeInMinutes = 5;
        public int GameTime => timeInMinutes;
        private Coroutine frozenTime;
        
        [Header("Score config")] 
        [SerializeField][Tooltip("How many points a tile is worth")]
        private float pointsPerTile = 10f;
        [SerializeField] [Tooltip("Seconds to reset combo value")]
        private float comboSecondsToReset = 3f;
        [SerializeField] [Tooltip("Combo multiplier")]
        private float pointsPerCombo = 1.2f;
        private int comboStreak = 0;
        private int score = 0;
        
        #region Lifecycle

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
            
        }

        #endregion

        #region Public

        public void UpdateScore(int tilesInMatch)
        {
            comboStreak ++;
            onComboUpdate?.Invoke(comboStreak);
            CancelInvoke(nameof(ComboTimeout));
            Invoke(nameof(ComboTimeout), comboSecondsToReset);
            
            score += (int)((pointsPerCombo * comboStreak) * (pointsPerTile * tilesInMatch));
            onScoreUpdate?.Invoke(score);
        }
        
        public void RequestFreezeTimeFor(float timeInSeconds)
        {
            onTimeFrozen?.Invoke(true);
            if(frozenTime != null)
                StopCoroutine(frozenTime);
            frozenTime = StartCoroutine(UnlockTimeFlowsAfter(timeInSeconds));
        }

        public void GameOver()
        {
            IsGameOver = true;
            onGameOver?.Invoke(score);
        }

        public void RequestGamePause(bool isPaused)
        {
            IsGamePaused = isPaused;
            Time.timeScale = isPaused ? 0 : 1;
            onGamePaused?.Invoke(IsGamePaused);
        }

        #endregion

        #region Private
        
        private void ComboTimeout()
        {
            comboStreak = 0;
            onComboUpdate?.Invoke(comboStreak);
        }
        private IEnumerator UnlockTimeFlowsAfter(float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            onTimeFrozen?.Invoke(false);
        }

        #endregion
    }
}