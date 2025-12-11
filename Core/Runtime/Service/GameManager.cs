using System;
using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Backend;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Runtime {
    // Event args for the initialization event.
    // Allows subscribers to register multiple asynchronous tasks
    // that must finish before the game can start.
    public class GameManager : MonoBehaviour {
        [SerializeField, Required] List<UserData> userDatas;

        // Events
        public static event EventHandler<GameInitEventArgs> OnGameInit;
        public static UnityEvent OnGameStart = new();

        void Start() => _ = InitializeGameAsync();

        /// <summary>
        /// Fires the OnGameInit event and awaits all initialization tasks
        /// registered by subscribers.
        /// </summary>
        async UniTask InitializeGameAsync() {
            if (OnGameInit != null) {
                // Create one shared args instance for all subscribers
                var initArgs = new GameInitEventArgs(userDatas);

                // Notify all subscribers
                OnGameInit.Invoke(this, initArgs);

                // Wait for all async tasks registered by subscribers
                if (initArgs.CompletionTasks.Any()) {
                    await UniTask.WhenAll(initArgs.CompletionTasks);
                }
            }

            OnGameStart?.Invoke();
        }

        void OnDestroy() {
            // Cleanup static events to avoid memory leaks during domain reloads
            OnGameInit = null;
            OnGameStart = null;
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