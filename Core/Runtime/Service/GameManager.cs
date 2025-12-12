using System;
using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Backend;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Runtime {
    // Event args for the initialization event.
    // Allows subscribers to register multiple asynchronous tasks
    // that must finish before the game can start.
    public class GameManager : MonoBehaviour {
        [SerializeField] bool testUsers;
        [SerializeField, Required, ShowIf("@testUsers")] List<UserData> testUserData;
        readonly List<UserData> _userDatas = new();

        // Events
        public event EventHandler<PreGameInitEventArgs> OnPreGameInit = delegate { };
        public event EventHandler<GameInitEventArgs> OnGameInit = delegate { };
        public event Action OnGameStart = delegate { };

        void Awake() { 
            // TESTING PURPOSE
            if (ServiceLocator.TryGet(out GameManager gameManager)) {
                Debug.LogError($"Duplicate GameManager detected. Keeping '{gameManager.gameObject.name}' and destroying this instance.", transform);
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }
        
        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        
        public void AddUserData(UserData userData) => _userDatas.Add(userData);
        public List<UserData> GetUserDatasCopy() => new(_userDatas);
        
        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InitializeGameAsync().Forget();

        /// <summary>
        /// Fires the OnPreGameInit, OnGameInit and OnGameStart events in sequence.
        /// Waits for all PreGameInit and GameInit tasks before firing OnGameStart.
        /// </summary>
        async UniTask InitializeGameAsync() {
            // Wait until the end of this frame (LastPostLateUpdate) so newly loaded scene objects can run Awake()/OnEnable() and subscribe to events
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            
            // Phase 1: PreGameInit (like Awake) - no parameters, wait for all subscribers
            var preInitArgs = new PreGameInitEventArgs();
            OnPreGameInit.Invoke(this, preInitArgs);
            
            if (preInitArgs.CompletionTasks.Any()) {
                await UniTask.WhenAll(preInitArgs.CompletionTasks);
            }
            
            // Phase 2: GameInit - with UserData, wait for all subscribers
            var userData = testUsers ? testUserData : _userDatas;
            var initArgs = new GameInitEventArgs(userData);
            OnGameInit.Invoke(this, initArgs);
            
            if (initArgs.CompletionTasks.Any()) {
                await UniTask.WhenAll(initArgs.CompletionTasks);
            }

            // Phase 3: GameStart - no parameters, fire and forget (don't wait)
            OnGameStart?.Invoke();
        }
        
        void OnDestroy() {
            ServiceLocator.Unregister<GameManager>();
            // Cleanup static events to avoid memory leaks during domain reloads
            OnPreGameInit = null;
            OnGameInit = null;
            OnGameStart = null;
        }
        
        public class PreGameInitEventArgs : EventArgs {
            // Subscribers can add any number of initialization tasks here.
            public List<UniTask> CompletionTasks { get; } = new();
        }
        
        public class GameInitEventArgs : EventArgs {
            public List<UserData> UserDatas { get; }

            // Subscribers can add any number of initialization tasks here.
            public List<UniTask> CompletionTasks { get; } = new();

            public GameInitEventArgs(List<UserData> userDatas) {
                UserDatas = userDatas;
            }
        }
    }
}