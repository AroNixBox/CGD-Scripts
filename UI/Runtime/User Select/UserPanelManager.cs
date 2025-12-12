using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class UserPanelManager : MonoBehaviour {
        [SerializeField, Required] UserCreationController creationController;
        [SerializeField, Required] UserHubController hubController;
        [SerializeField, Required] LoadingScreenController loadController;

        void OnEnable() {
            creationController.OnSubmitUser += ShowHub;
            hubController.OnAddUser += ShowCreation;
            hubController.OnStartGame += PrepareStartGame;
        }
        void OnDisable() {
            creationController.OnSubmitUser -= ShowHub;
            hubController.OnAddUser -= ShowCreation;
            hubController.OnStartGame -= PrepareStartGame;
        }

        void PrepareStartGame() {
            loadController.LoadSceneAsync("Playground_Nixon").Forget();
        }

        void Start() {
            ShowHub();
            creationController.Hide();
            loadController.Hide();
        }

        void ShowCreation() => creationController.Show();

        void ShowHub() => hubController.Show();
    }
}