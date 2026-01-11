using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Runtime.Menus {
    public class PauseMenuView : MonoBehaviour {
        [SerializeField, Required] GameObject pauseMenuUi;
        [SerializeField, Required] Button resumeButton;
        [SerializeField, Required] Button mainMenuButton;
        
        public bool IsVisible => pauseMenuUi.activeSelf;

        public void Show() {
            pauseMenuUi.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        
        public void Hide() {
            pauseMenuUi.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
        }
        
        public void BindButtons(UnityAction onResume, UnityAction onMainMenu) {
            resumeButton.onClick.AddListener(onResume);
            mainMenuButton.onClick.AddListener(onMainMenu);
        }
        
        public void UnbindButtons(UnityAction onResume, UnityAction onMainMenu) {
            resumeButton.onClick.RemoveListener(onResume);
            mainMenuButton.onClick.RemoveListener(onMainMenu);
        }
    }
}

