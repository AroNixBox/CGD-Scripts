using System;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.Runtime {
    public class CinematicGameOver : MonoBehaviour {
        [SerializeField, Required] CinemachineCamera winnerCam;
        AuthorityManager _authorityManager;
        const int ActiveValue = 20;


        void Start() {
            if(ServiceLocator.TryGet(out _authorityManager))
                _authorityManager.OnLastEntityRemaining += EnableWinnerCam;
        }

        void EnableWinnerCam(AuthorityEntity obj) {
            if (winnerCam == null) return;
            if (obj == null) return; // No Winner

            winnerCam.Target = new CameraTarget {
                TrackingTarget = obj.transform
            };

            winnerCam.Priority = ActiveValue;
        }

        void OnDestroy() {
            if(ServiceLocator.TryGet(out _authorityManager))
                _authorityManager.OnLastEntityRemaining -= EnableWinnerCam;
        }
    }
}
