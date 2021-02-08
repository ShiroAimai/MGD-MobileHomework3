using Models;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Ui
{
    public class PowerUpSelectionController : MonoBehaviour
    {
        [SerializeField] private Image bombImage;
        [SerializeField] private Image freezeImage;

        private string currentSelection;
        
        void Start()
        {
            var previouslySelectedPowerUp = PlayerPrefs.GetString(PlayerPrefHelper.PowerUpKey, null);
            UpdateCurrentPowerUpSelection(string.IsNullOrEmpty(previouslySelectedPowerUp) ? PowerUp.Type.Bomb.ToPowerUpString() : previouslySelectedPowerUp);
        }

        public void UpdatePowerUpSelection(string powerUp)
        {
            if (powerUp != null && powerUp == currentSelection) return;
            UpdateCurrentPowerUpSelection(powerUp);
        }

        private void UpdateCurrentPowerUpSelection(string newSelection)
        {
            currentSelection = newSelection;

            PlayerPrefs.SetString(PlayerPrefHelper.PowerUpKey, currentSelection); //default init

            var isBombPowerUp = currentSelection == PowerUp.Type.Bomb.ToPowerUpString();
            bombImage.enabled = isBombPowerUp;
            freezeImage.enabled = !isBombPowerUp;
        }
    }
}
