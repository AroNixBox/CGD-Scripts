using System;
using Core.Runtime.Authority;
using Core.Runtime.Cinematics;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime {
    // Manages the Turn Based Loop
    public class RoundInitializer : MonoBehaviour {
        [SerializeField] bool cinematicOnboarding;
        [SerializeField, Required] AuthorityManager authorityManager;
        [SerializeField, Required] DollyCameraController dollyCameraController;

        async void Start() {
            if (!cinematicOnboarding) {
                authorityManager.Init();
                return;
            }
            
            try {
                // 1. Camera Dolly
                await dollyCameraController.MoveDollyToTarget(this.GetCancellationTokenOnDestroy()); // Scene 
                authorityManager.Init();
            }
            catch (OperationCanceledException e) {
                // Noop, Scene was switched while the Initialization was running (its okay :))
            }
        }
    }
}
