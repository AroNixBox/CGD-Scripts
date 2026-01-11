using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        [SerializeField, Required] CinemachineCamera impactCamera;
        [SerializeField, Required] CinemachineTargetGroup impactTargetGroup;
        
        

        [Tooltip("Typically the ModelRoot that is actively rotated")]
        [SerializeField, Required] Transform rotationTarget;
        CinemachineTargetTracker _targetTracker;
        CinemachineBrain _brain;
        
        const int HighPriority = 10;
        const int LowPriority = 0;

        void Awake() {
            _targetTracker = bulletCamera.GetComponent<CinemachineTargetTracker>(); // TODO: Cache
        }

        void Start() {
            var mainCam = UnityEngine.Camera.main;
            if (mainCam != null)
                _brain = mainCam.GetComponent<CinemachineBrain>();
        }

        public Transform GetActiveCameraTransform() {
            return firstPersonCamera.Priority > thirdPersonCamera.Priority 
                ? firstPersonCamera.transform 
                : thirdPersonCamera.transform;
        }
        
        // TODO: Set the new camera forward to the old camera forward to avoid mismatch
        public async UniTask SwitchToControllableCameraMode(ECameraMode mode) {
            if (mode == ECameraMode.FirstPerson) {
                // Sync camera state before blend to avoid rotation jump
                SyncCameraRotation(firstPersonCamera, thirdPersonCamera);
                SetCameraPriorities(firstPerson: HighPriority, thirdPerson: LowPriority);
            }
            else if (mode == ECameraMode.ThirdPerson) {
                // Sync camera state before blend to avoid rotation jump
                SyncCameraRotation(thirdPersonCamera, firstPersonCamera);
                SetCameraPriorities(firstPerson: LowPriority, thirdPerson: HighPriority);
            }
            
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
    
            // Wait until blend ends
            await UniTask.WaitUntil(() => !_brain.IsBlending);

            return;
            
            void SetCameraPriorities(int firstPerson, int thirdPerson) {
                firstPersonCamera.Priority = firstPerson;
                thirdPersonCamera.Priority = thirdPerson;
            }
            
            void SyncCameraRotation(CinemachineCamera target, CinemachineCamera source) {
                // Force Cinemachine to update the source camera's state first
                source.ForceCameraPosition(source.State.GetFinalPosition(), source.State.GetFinalOrientation());
                
                // Then sync the target camera to match
                target.ForceCameraPosition(target.transform.position, source.State.GetFinalOrientation());
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
            if (bulletCamera == null) return; // BulletCam was destroyed
            
            bulletCamera.Priority = LowPriority;
            bulletCamera.transform.localPosition = Vector3.zero;
            _targetTracker.FollowTarget = null;
        }

        // TODO: FIX camera zoom, its was too close....!!!
        public void EnableImpactCamera(List<Transform> trackingTargets) {
            if (impactCamera == null) return;
            if (impactTargetGroup == null) return;


            foreach (var t in trackingTargets) {
                CinemachineTargetGroup.Target target = new() {
                    Object = t,
                    Radius = .5f,
                    Weight = 1
                };
                impactTargetGroup.Targets.Add(target);
            }
            impactCamera.Priority = HighPriority;
        }
        
        public void ResetImpactCamera() {
            if (impactCamera == null) return;
            if (impactTargetGroup == null) return;

            impactCamera.Priority = LowPriority;
            impactTargetGroup.Targets.Clear();
        }
    }
}