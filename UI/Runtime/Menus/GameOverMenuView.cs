using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Runtime.Menus {
    public class GameOverMenuView : MonoBehaviour {
        [SerializeField, Required] Transform gameOverUi;
        [SerializeField, Required] TextMeshProUGUI winnerText;
        [SerializeField, Required] Button restartButton;
        [SerializeField, Required] Button menuButton;
        
        public void Show(string winnerName) {
            if (gameOverUi == null) {
                Debug.LogError("Game Over UI is null");
                return;
            }
            if (winnerText == null) {
                Debug.LogError("Winner Text is null");
                return;
            }
            
            gameOverUi.gameObject.SetActive(true);
            winnerText.text = $"{winnerName} Wins!";
        }
        
        public void Hide() {
            if (gameOverUi == null) {
                Debug.LogError("Game Over UI is null");
                return;
            }
            
            gameOverUi.gameObject.SetActive(false);
        }
        
        public void BindButtons(UnityAction onRestart, UnityAction onMenu) {
            restartButton.onClick.AddListener(onRestart);
            menuButton.onClick.AddListener(onMenu);
        }
    }
}