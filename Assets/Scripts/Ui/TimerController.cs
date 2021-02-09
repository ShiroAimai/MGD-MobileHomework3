using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class TimerController : MonoBehaviour
    {
     
        [SerializeField] private Text timerTxt;
        [SerializeField] private GameObject frozenTimer;

        private float timeInSeconds = 0f;
        private bool isTimeFlowing = true;

        private void Start()
        {
            timeInSeconds = (GameManager.Instance.GameTime * 60);
            GameManager.Instance.onTimeFrozen += OnTimeFrozen;
        }

        private void Update()
        {
            if(isTimeFlowing)
                OnTimerUpdate(Time.deltaTime);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.onTimeFrozen -= OnTimeFrozen;
        }

        private void OnTimeFrozen(bool isFrozen)
        {
            isTimeFlowing = !isFrozen;
            frozenTimer.SetActive(isFrozen);
        }

        private void OnTimerUpdate(float elapsed)
        {
            timeInSeconds -= elapsed;
            
            int minutes = ((int) timeInSeconds / 60);
            int seconds = (int) timeInSeconds % 60;
            
            timerTxt.text = $"{minutes:00}:{seconds:00}";
        }
    }
}