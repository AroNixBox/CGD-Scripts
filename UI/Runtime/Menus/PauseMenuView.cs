using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Runtime.Menus {
    public class PauseMenuView : MonoBehaviour {
        [SerializeField, Required] GameObject pauseMenuUi;
        [SerializeField, Required] Button resumeButton;
        [SerializeField, Required] Button restartButton;
        [SerializeField, Required] Button settingsButton;
        [SerializeField, Required] Button helpButton;
        [SerializeField, Required] Button mainMenuButton;
        [SerializeField, Required] Button quitButton;

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
        
        public void BindButtons(UnityAction onResume, UnityAction onRestart, UnityAction onSettings, UnityAction onHelp, UnityAction onMainMenu, UnityAction onQuit) {
            resumeButton.onClick.AddListener(onResume);
            restartButton.onClick.AddListener(onRestart);
            settingsButton.onClick.AddListener(onSettings);
            helpButton.onClick.AddListener(onHelp);
            mainMenuButton.onClick.AddListener(onMainMenu);
            quitButton.onClick.AddListener(onQuit);
        }
        
        public void UnbindButtons(UnityAction onResume, UnityAction onRestart, UnityAction onSettings, UnityAction onHelp, UnityAction onMainMenu, UnityAction onQuit) {
            resumeButton.onClick.RemoveListener(onResume);
            restartButton.onClick.RemoveListener(onRestart);
            settingsButton.onClick.RemoveListener(onSettings);
            helpButton.onClick.RemoveListener(onHelp);
            mainMenuButton.onClick.RemoveListener(onMainMenu);
            quitButton.onClick.RemoveListener(onQuit);
        }
    }
}

