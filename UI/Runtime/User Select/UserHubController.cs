using System;
using System.IO;
using Core.Runtime;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Runtime {
    public class UserHubController : MonoBehaviour {
        [SerializeField, Required] UserHubView view;
        public event Action OnAddUser = delegate { };

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

        void StartGame() {
            const string sceneName = "Playground_Nixon";
            if (!IsSceneInBuild(sceneName)) {
                Debug.LogError($"Scene `{sceneName}` not found in Build Settings.");
                return;
            }
            StartGameAsync(sceneName).Forget();
        }

        async UniTask StartGameAsync(string sceneName) {
            var op = SceneManager.LoadSceneAsync(sceneName);
            // Dont activate instantly
            if (op != null) {
                op.allowSceneActivation = false;
                
                // Activate Loading UI

                // Wait for laod
                await UniTask.WaitUntil(() => op.progress >= 0.9f);

                // optional: hier Loading UI schließen / Übergang starten

                // Activate Scene
                op.allowSceneActivation = true;
                await UniTask.WaitUntil(() => op.isDone);
            }
        }

        bool IsSceneInBuild(string sceneName) {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(name, sceneName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        void AddPressedUser() {
            OnAddUser.Invoke();
            
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
