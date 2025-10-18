using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Runtime.Player.Camera {
    // TODO: Disable Camera Controls when losing authority, re-enable when gaining authority.
    public class PlayerCameraControls : MonoBehaviour {
        [SerializeField, Required] CinemachineCamera thirdPersonCamera;
        [SerializeField, Required] CinemachineCamera firstPersonCamera;
        
        const int HighPriority = 10;
        const int LowPriority = 0;
        public Transform GetActiveCameraTransform() {
            return firstPersonCamera.Priority > thirdPersonCamera.Priority 
                ? firstPersonCamera.transform 
                : thirdPersonCamera.transform;
        }
        
        // TODO: Set the new camera forward to the old camera forward to avoid mismatch
        public void SwitchToCameraMode(CameraMode mode) {
            thirdPersonCamera.Priority = mode == CameraMode.ThirdPerson 
                ? HighPriority 
                : LowPriority;
            firstPersonCamera.Priority = mode == CameraMode.ThirdPerson
                ? LowPriority
                : HighPriority;
        }
        public void ResetCameras() {
            thirdPersonCamera.Priority = LowPriority;
            firstPersonCamera.Priority = LowPriority;
        }
        public enum CameraMode {
            FirstPerson,
            ThirdPerson
        }
        
    }
}