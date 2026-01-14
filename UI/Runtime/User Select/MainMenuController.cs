using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class MainMenuController : MonoBehaviour {
        [SerializeField, Required] MainMenuView view;
        [SerializeField, Required] LoadingScreenController loadingScreenController;

        void OnEnable() {
            view.OnStartClicked += LoadUserHub;
        }

        void LoadUserHub() {
            _ = loadingScreenController.LoadSceneAsync("User Hub");
        }
        
        void OnDisable() {
            view.OnStartClicked -= LoadUserHub;
        }
    }
}
