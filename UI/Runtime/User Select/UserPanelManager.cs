using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class UserPanelManager : MonoBehaviour {
        [SerializeField, Required] UserCreationController creationController;
        [SerializeField, Required] UserHubController hubController;

        void OnEnable() {
            creationController.OnSubmitUser += ShowHub;
            hubController.OnAddUser += ShowCreation;
        }

        void Start() {
            ShowHub();
            creationController.Hide();
        }

        void ShowCreation() => creationController.Show();

        void ShowHub() => hubController.Show();
        
        void OnDisable() {
            creationController.OnSubmitUser -= ShowHub;
            hubController.OnAddUser -= ShowCreation;
        }
    }
}