using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Runtime.Menus {
    public class GameOverMenuView : MonoBehaviour {
        [SerializeField, Required] Transform gameOverUi;
        [SerializeField, Required] Button restartButton;
        
        public void Show() {
            gameOverUi.gameObject.SetActive(true);
        }
        
        public void Hide() {
            gameOverUi.gameObject.SetActive(false);
        }
        
        public void BindButtons(UnityAction onRestart) {
            restartButton.onClick.AddListener(onRestart);
        }
    }
}