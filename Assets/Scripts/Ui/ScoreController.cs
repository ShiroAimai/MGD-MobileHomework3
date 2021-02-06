using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class ScoreController : MonoBehaviour
    {
        [SerializeField] private Text scoreTxt;
        private void Start()
        {
            GameManager.Instance.onScoreUpdate += OnScoreUpdate;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.onScoreUpdate -= OnScoreUpdate;
        }

        private void OnScoreUpdate(int newScore)
        {
            scoreTxt.text = newScore.ToString();
        }
    }
}