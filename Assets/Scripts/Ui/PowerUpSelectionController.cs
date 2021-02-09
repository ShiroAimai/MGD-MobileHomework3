using Models;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui
{
    public class PowerUpSelectionController : MonoBehaviour
    {
        [SerializeField] private Image powerUp;
        [SerializeField] private Sprite bomb;
        [SerializeField] private Sprite freeze;
        
        private string currentSelection;
        
        void Start()
        {
            var previouslySelectedPowerUp = PlayerPrefs.GetString(PlayerPrefHelper.PowerUpKey, null);
            UpdateCurrentPowerUpSelection(string.IsNullOrEmpty(previouslySelectedPowerUp) ? PowerUp.Type.Bomb.ToPowerUpString() : previouslySelectedPowerUp);
        }

        public void UpdatePowerUpSelection()
        {
            string bombPowerUpName =  PowerUp.Type.Bomb.ToPowerUpString();
            string freezePowerUpName =  PowerUp.Type.Freeze.ToPowerUpString();
            string nextPowerUpSelection = currentSelection == bombPowerUpName ? freezePowerUpName : bombPowerUpName;
            UpdateCurrentPowerUpSelection(nextPowerUpSelection);
        }

        private void UpdateCurrentPowerUpSelection(string newSelection)
        {
            currentSelection = newSelection;

            PlayerPrefs.SetString(PlayerPrefHelper.PowerUpKey, currentSelection); //default init

            var isBombPowerUp = currentSelection == PowerUp.Type.Bomb.ToPowerUpString();
            powerUp.sprite = isBombPowerUp ? bomb : freeze;
        }
    }
}
