using System;
using Core.Runtime;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime {
    public class UserHubController : MonoBehaviour {
        [SerializeField, Required] UserHubView view;
        public event Action OnAddUser = delegate { };
        public event Action OnStartGame = delegate { };

        void OnEnable() {
            view.OnUserAddPressed += OpenAddUserPanel;
            view.OnStartGamePressed += StartGame;
        }
        void OnDisable() {
            view.OnUserAddPressed -= OpenAddUserPanel;
            view.OnStartGamePressed -= StartGame;
        }

        void OpenAddUserPanel() {
            OnAddUser.Invoke();
            
            // Erase all Entries and hide self
            view.DespawnHubEntries();
            view.Hide();
        }

        void StartGame() {
            OnStartGame.Invoke();
            
            // Erase all Entries and hide self
            view.DespawnHubEntries();
            view.Hide();
        }

        public void Show() {
            if (!ServiceLocator.TryGet(out GameManager gameManager)) {
                Debug.LogError("Game Manager is null, cant load Users");
                return;
            }

            var users = gameManager.GetUserDatasCopy();
            foreach (var user in users) {
                view.SpawnHubEntry(user);
            }
            
            view.Show();
        }
    }
}
