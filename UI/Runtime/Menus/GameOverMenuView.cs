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
            gameOverUi.gameObject.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            winnerText.text = $"{winnerName} Wins!";
        }
        
        public void Hide() {
            gameOverUi.gameObject.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }
        
        public void BindButtons(UnityAction onRestart, UnityAction onMenu) {
            restartButton.onClick.AddListener(onRestart);
            menuButton.onClick.AddListener(onMenu);
        }
    }
}