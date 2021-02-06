using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class TimerController : MonoBehaviour
    {
     
        [SerializeField] private Text timerTxt;
        
        private void Start()
        {
            GameManager.Instance.onTimeUpdate += OnTimerUpdate;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.onTimeUpdate -= OnTimerUpdate;
        }
        
        private void OnTimerUpdate(float timeInSeconds)
        {
            int minutes = ((int) timeInSeconds / 60);
            int seconds = (int) timeInSeconds % 60;
            
            timerTxt.text = $"{minutes} : {seconds}";
        }
    }
}