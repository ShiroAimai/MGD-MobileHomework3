using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui
{
    public class PowerUpSelectionController : MonoBehaviour
    {
        [SerializeField] private List<PowerUp.Type> powerUps;
        [SerializeField] private Text selectedPowerUp;
        
        private int _currentSelectionIndex = 0;
        
        void Start()
        {
            var previouslySelectedPowerUp = PlayerPrefs.GetString(PlayerPrefHelper.PowerUpKey, null);
            if (!string.IsNullOrEmpty(previouslySelectedPowerUp))
                _currentSelectionIndex = powerUps.IndexOf(PowerUp.FromString(previouslySelectedPowerUp)); 
            UpdateCurrentPowerUpSelection();
        }

        public void UpdatePowerUpSelection()
        {
            if (_currentSelectionIndex + 1 < powerUps.Count)
                _currentSelectionIndex++;
            else _currentSelectionIndex = 0;
            UpdateCurrentPowerUpSelection();
        }

        private void UpdateCurrentPowerUpSelection()
        {
            var selectedPowerUpValue = powerUps[_currentSelectionIndex].ToPowerUpString();
            PlayerPrefs.SetString(PlayerPrefHelper.PowerUpKey, selectedPowerUpValue); //default init
            selectedPowerUp.text = selectedPowerUpValue.ToUpper();
        }
    }
}
