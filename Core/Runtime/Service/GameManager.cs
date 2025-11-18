using System;
using Core.Runtime.Authority;
using Core.Runtime.Cinematics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Runtime {
    // Manages the Turn Based Loop
    public class GameManager : MonoBehaviour {
        [SerializeField, Required] AuthorityManager authorityManager;
        [SerializeField, Required] DollyCameraController dollyCameraController;

        void Start() {
            // 1. Camera Dolly
            dollyCameraController.InitDolly();
            dollyCameraController.OnDollyCameraTargetReached += authorityManager.Init;
        }

        void OnDestroy() {
            dollyCameraController.OnDollyCameraTargetReached -= authorityManager.Init;
        }
    }
}
