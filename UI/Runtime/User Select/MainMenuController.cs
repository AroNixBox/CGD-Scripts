using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime {
    public class MainMenuController : MonoBehaviour {
        [SerializeField, Required] MainMenuView view;
        [SerializeField, Required] Button quitButton;
        [SerializeField, Required] LoadingScreenController loadingScreenController;

        void OnEnable() {
            view.OnStartClicked += LoadUserHub;
            quitButton.onClick.AddListener(QuitClicked);
        }

        void LoadUserHub() {
            _ = loadingScreenController.LoadSceneAsync("User Hub");
        }

        void QuitClicked() {
            Application.Quit();
        }

        void OnDisable() {
            view.OnStartClicked -= LoadUserHub;
            quitButton.onClick.RemoveListener(QuitClicked);
        }
    }
}
