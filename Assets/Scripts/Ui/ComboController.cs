using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    [RequireComponent(typeof(Animator))]
    public class ComboController : MonoBehaviour
    {
        [SerializeField] private Text comboTxt;
        [SerializeField] private GameObject comboLabel;
        [SerializeField] private List<string> comboValues;
        
        private Animator _animator;
        private void Start()
        {
            _animator = GetComponent<Animator>();
            GameManager.Instance.onComboUpdate += OnComboUpdate;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.onComboUpdate -= OnComboUpdate;
        }

        private void OnComboUpdate(int newComboStreak)
        {
            bool isPlayerInACombo = newComboStreak >= 1;
            if(isPlayerInACombo)
                _animator.SetTrigger("Update");
            comboLabel.SetActive(isPlayerInACombo);
            comboTxt.text = newComboStreak < comboValues.Count ? comboValues[newComboStreak] : comboValues[comboValues.Count - 1];
        }
    }
}