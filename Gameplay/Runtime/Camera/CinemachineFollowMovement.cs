using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Runtime.Camera {
    [RequireComponent(typeof(CinemachineOrbitalFollow), typeof(CinemachineTargetTracker))]
    public class CameraFollowMovement : MonoBehaviour {
        [SerializeField] float rotationSpeed = 5f;
        [SerializeField] float minVelocityThreshold = 0.1f;
        
        CinemachineOrbitalFollow _orbitalFollow;
        CinemachineTargetTracker _targetTracker;
        float _targetHorizontalAxis;
        
        Rigidbody _targetRb; // The rigidbody in which position we rotate

        void Awake() {
            _orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            _targetTracker = GetComponent<CinemachineTargetTracker>();
        }

        void OnEnable() {
            _targetTracker.OnTargetChanged += SetRigidbodyTarget;
        }

        void SetRigidbodyTarget(Transform newTarget) {
            // NewTarget is null or has no RB:
            if (newTarget == null || !newTarget.TryGetComponent(out Rigidbody rb)) {
                _targetRb = null;
                return;
            }

            _targetRb = rb;
        }

        void LateUpdate() {
            if (_orbitalFollow == null || _targetRb == null)
                return;

            // Camera look in Target forward
            var velocity = _targetRb.linearVelocity;
            var velocityXZ = new Vector3(velocity.x, 0f, velocity.z);

            if (velocityXZ.magnitude > minVelocityThreshold) {
                var movementAngle = Mathf.Atan2(velocityXZ.x, velocityXZ.z) * Mathf.Rad2Deg;
                
                _targetHorizontalAxis = movementAngle;
                
                while (_targetHorizontalAxis > 180f) _targetHorizontalAxis -= 360f;
                while (_targetHorizontalAxis < -180f) _targetHorizontalAxis += 360f;
            }

            var currentAxis = _orbitalFollow.HorizontalAxis.Value;
            var newAxis = Mathf.LerpAngle(currentAxis, _targetHorizontalAxis, Time.deltaTime * rotationSpeed);
            
            _orbitalFollow.HorizontalAxis.Value = newAxis;
        }

        void OnDisable() {
            _targetTracker.OnTargetChanged -= SetRigidbodyTarget;
        }

        // Debug
        void OnDrawGizmos() {
            if (_targetRb == null || !Application.isPlaying) return;
            var velocity = _targetRb.linearVelocity;
            var velocityXZ = new Vector3(velocity.x, 0f, velocity.z);

            if (!(velocityXZ.magnitude > minVelocityThreshold)) return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, velocityXZ.normalized * 2f);
                    
            Gizmos.color = Color.red;
            var cameraDirection = Quaternion.Euler(0, _targetHorizontalAxis, 0) * Vector3.forward;
            Gizmos.DrawRay(transform.position, cameraDirection * 2f);
        }
    }
}
