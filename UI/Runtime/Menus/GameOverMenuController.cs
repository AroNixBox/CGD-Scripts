using System;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Runtime.Menus {
    public class GameOverMenuController : MonoBehaviour {
        [SerializeField, Required] GameOverMenuView view;
        AuthorityManager _authorityManager;

        void OnEnable() {
            view.BindButtons(
                onRestart: () => SceneManager.LoadScene(SceneManager.GetActiveScene().name),
                onMenu: () => SceneManager.LoadScene("Scenes/User Hub")
                );
            
            view.Hide();
        }

        void Start() {
            if(ServiceLocator.TryGet(out _authorityManager))
                _authorityManager.OnLastEntityRemaining += OpenMenu;
        }

        
        void OpenMenu(AuthorityEntity entity) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            view.Show(entity.UserData.Username);
        }


        void OnDestroy() {
            if(_authorityManager != null)
                _authorityManager.OnLastEntityRemaining -= OpenMenu;
        }
    }
}
