using UnityEngine;
using System;
using Unity.Cinemachine;

namespace Gameplay.Runtime.Camera {
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineTargetTracker : MonoBehaviour {
        CinemachineCamera _virtualCamera;
        public event Action<Transform> OnTargetChanged;

        Transform _currentTarget;

        public Transform FollowTarget {
            get => _currentTarget;
            set {
                if (_currentTarget != value) {
                    _currentTarget = value;
                    if (_virtualCamera != null)
                        _virtualCamera.Follow = value;
                    OnTargetChanged?.Invoke(value);
                }
            }
        }

        void Awake() {
            _virtualCamera = GetComponent<CinemachineCamera>();
            
            if (_virtualCamera != null)
                _currentTarget = _virtualCamera.Follow;
        }
    }

}
