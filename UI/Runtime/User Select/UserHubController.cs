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
            InitializeHubEntries().Forget();
            view.OnUserAddPressed += AddPressedUser;
            view.OnStartGamePressed += StartGame;
        }
        void OnDisable() {
            view.DespawnHubEntries();
            view.OnUserAddPressed -= AddPressedUser;
            view.OnStartGamePressed -= StartGame;
        }

        void AddPressedUser() {
            OnAddUser.Invoke();
            
            // Deinitialize Self
            view.DespawnHubEntries();
            view.Hide();
        }

        void StartGame() {
            OnStartGame.Invoke();
            
            // Deinitialize Self
            view.DespawnHubEntries();
            view.Hide();
        }

        async UniTask InitializeHubEntries() {
            GameManager gameManager = null;
            await UniTask.WaitUntil(() => ServiceLocator.TryGet(out gameManager));

            var users = gameManager.GetUserDatasCopy();
            foreach (var user in users) {
                view.SpawnHubEntry(user);
            }
        }

        public void Show() {
            InitializeHubEntries().Forget();
            view.Show();
        }
    }
}
