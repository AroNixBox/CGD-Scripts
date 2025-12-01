using System;
using System.Collections.Generic;
using Core.Runtime.Authority;
using Core.Runtime.Backend;
using Core.Runtime.Cinematics;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime {
    // Manages the Turn Based Loop
    public class GameManager : MonoBehaviour {
        [SerializeField] bool cinematicOnboarding;
        [SerializeField, Required] AuthorityManager authorityManager;
        [SerializeField, Required] DollyCameraController dollyCameraController; 
        
        // TODO: Should be injected from selection screen
        [SerializeField, Required] List<UserData> userDatas;

        async void Start() {
            if (!cinematicOnboarding) {
                InitializeGame();
                return;
            }
            
            try {
                // 1. Camera Dolly
                await dollyCameraController.MoveDollyToTarget(this.GetCancellationTokenOnDestroy()); // Scene 
                InitializeGame();
            }
            catch (OperationCanceledException e) {
                // Noop, Scene was switched while the Initialization was running (its okay :))
            }
        }

        void InitializeGame() {
            foreach (var userData in userDatas) {
                authorityManager.InitializeEntity(userData);
            }

            authorityManager.StartFlow();
        }
    }
}
