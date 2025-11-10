using System;
using Gameplay.Runtime.Camera;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Runtime.Player.Camera {
    // TODO: Disable Camera Controls when losing authority, re-enable when gaining authority.
    public class PlayerCameraControls : MonoBehaviour {
        [SerializeField, Required] CinemachineCamera thirdPersonCamera;
        [SerializeField, Required] CinemachineCamera firstPersonCamera;
        [SerializeField, Required] CinemachineCamera bulletCamera;

        [Tooltip("Typically the ModelRoot that is actively rotated")]
        [SerializeField, Required] Transform rotationTarget;
        CinemachineTargetTracker _targetTracker;
        
        const int HighPriority = 10;
        const int LowPriority = 0;

        void Awake() {
            _targetTracker = bulletCamera.GetComponent<CinemachineTargetTracker>(); // TODO: Cache
        }

        public Transform GetActiveCameraTransform() {
            return firstPersonCamera.Priority > thirdPersonCamera.Priority 
                ? firstPersonCamera.transform 
                : thirdPersonCamera.transform;
        }
        
        // TODO: Set the new camera forward to the old camera forward to avoid mismatch
        public void SwitchToControllableCameraMode(ECameraMode mode) {
            if (mode == ECameraMode.FirstPerson) {
                firstPersonCamera.transform.forward = thirdPersonCamera.transform.forward;
                SetCameraPriorities(firstPerson: HighPriority, thirdPerson: LowPriority);
            } 
            else if (mode == ECameraMode.ThirdPerson) {
                thirdPersonCamera.transform.forward = firstPersonCamera.transform.forward;
                SetCameraPriorities(firstPerson: LowPriority, thirdPerson: HighPriority);
            }

            return;
            void SetCameraPriorities(int firstPerson, int thirdPerson) {
                firstPersonCamera.Priority = firstPerson;
                thirdPersonCamera.Priority = thirdPerson;
            }
        }
        public void ResetControllableCameras() {
            thirdPersonCamera.Priority = LowPriority;
            firstPersonCamera.Priority = LowPriority;
        }
        public enum ECameraMode {
            FirstPerson,
            ThirdPerson
        }

        public void EnableBulletCamera(Transform target) {
            bulletCamera.transform.forward = rotationTarget.forward; // Look at player forward
            _targetTracker.FollowTarget = target;
            bulletCamera.Priority = HighPriority;
        }
        public void ResetBulletCamera() {
            bulletCamera.Priority = LowPriority;
            bulletCamera.transform.localPosition = Vector3.zero;
            _targetTracker.FollowTarget = null;
        }
    }
}