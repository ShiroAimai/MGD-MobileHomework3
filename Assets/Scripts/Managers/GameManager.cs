using System;
using System.Collections;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance = null;

        public Action<float> onTimeUpdate;
        public Action<int> onScoreUpdate;
        public Action<int> onComboUpdate;

        [Header("Time config")]
        private bool isTimeFlowing = true;
        [SerializeField][Tooltip("Game time. In minutes")] private float timeInMinutes = 5f;
        private float currentTimeInSeconds;

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

        private void Start()
        {
            currentTimeInSeconds = timeInMinutes * 60; 
            InvokeRepeating(nameof(UpdateGameTime), 0f, 1f);
        }

        #endregion

        #region Public

        public void UpdateScore(int tilesInMatch)
        {
            comboStreak += tilesInMatch / 3;
            onComboUpdate?.Invoke(comboStreak);
            CancelInvoke(nameof(ComboTimeout));
            Invoke(nameof(ComboTimeout), comboSecondsToReset);
            
            score += (int)((pointsPerCombo * comboStreak) * (pointsPerTile * tilesInMatch));
            onScoreUpdate?.Invoke(score);
        }
        
        public void RequestFreezeTimeFor(float timeInSeconds)
        {
            isTimeFlowing = false;
            StopCoroutine(UnlockTimeFlowsAfter(timeInSeconds));
            StartCoroutine(UnlockTimeFlowsAfter(timeInSeconds));
        }

        #endregion

        #region Private

        private void UpdateGameTime()
        {
            if (!isTimeFlowing) return;
            currentTimeInSeconds--;
            onTimeUpdate?.Invoke(currentTimeInSeconds);
        }
        private void ComboTimeout()
        {
            comboStreak = 0;
            onComboUpdate?.Invoke(comboStreak);
        }
        private IEnumerator UnlockTimeFlowsAfter(float timeInSeconds)
        {
            yield return new WaitForSeconds(timeInSeconds);
            isTimeFlowing = true;
        }

        #endregion
    }
}