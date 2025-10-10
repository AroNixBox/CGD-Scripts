using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime {
    public class ActiveRagdollStabilizer : MonoBehaviour {
        [Header("References")]
        [Tooltip("Die Hips des Physical Models (mit Rigidbody)")]
        [SerializeField, Required] Transform hips;

        [Header("Settings")] 
        [Title("Base Config")]
        [Tooltip("Desired height of the hips above the ground")]
        [SerializeField] float desiredHipHeight = 1.0f;
        [Tooltip("Only if the Ray hits the ground, we use stabilization")]
        [SerializeField] float raycastDistance = 5.0f;
    
        [Title("Force Settings")]
        [Tooltip("Force for vertical stabilization")]
        [SerializeField] float heightForce = 500f;
        
        [Tooltip("Damp fast vertical movements")]
        [SerializeField] float heightDamping = 50f;
    
        [Tooltip("Torque applied to align the character's up direction with the ground normal. " +
                 "Example: If the character stands on a slope, this torque helps keep them upright relative to the slope.")]
        [SerializeField] float rotationTorque = 100f;
    
        [Tooltip("Damping for rotational movements, to slow them down")]
        [SerializeField] float rotationDamping = 10f;
    
        [Header("Debug")]
        [SerializeField] bool showDebugRays = true;
    
        Rigidbody _hipsRb;
        RaycastHit _groundHit;
        bool _isGrounded;

        void Start() {
            if (hips == null) {
                Debug.LogError("Hips Transform not assigned!");
                enabled = false;
                return;
            }
            
            _hipsRb = hips.GetComponent<Rigidbody>();
            if (_hipsRb == null) {
                Debug.LogError("Hips have no Rigidbody component!");
                enabled = false;
                return;
            }
        }

        void FixedUpdate() {
            CheckGround();
            
            if (_isGrounded) {
                StabilizeHeight();
                StabilizeRotation();
            }
        }

        void CheckGround() {
            var rayStart = hips.position;
            var rayDirection = Vector3.down;
            
            _isGrounded = Physics.Raycast(rayStart, rayDirection, out _groundHit, raycastDistance);
            
            if (showDebugRays) {
                Debug.DrawRay(rayStart, rayDirection * raycastDistance, _isGrounded ? Color.green : Color.red);
                
                if (_isGrounded) {
                    Debug.DrawRay(_groundHit.point, _groundHit.normal * 2f, Color.blue);
                }
            }
        }

        void StabilizeHeight() {
            var currentHeight = hips.position.y - _groundHit.point.y;
            var heightError = desiredHipHeight - currentHeight;
    
            if (currentHeight > desiredHipHeight * 2f) return;
    
            var verticalVelocity = Vector3.Dot(_hipsRb.linearVelocity, Vector3.up);
    
            var force = (heightError * heightForce) - (verticalVelocity * heightDamping);
    
            _hipsRb.AddForce(Vector3.up * force, ForceMode.Force);
    
            
            if (showDebugRays) {
                Vector3 targetPos = _groundHit.point + Vector3.up * desiredHipHeight;
                Debug.DrawLine(hips.position, targetPos, Color.yellow);
            }
        }

        void StabilizeRotation() {
            var groundNormal = _groundHit.normal;
            
            var targetUp = groundNormal;
            
            var currentUp = hips.up;
            
            var rotationAxis = Vector3.Cross(currentUp, targetUp);
            var rotationAngle = Vector3.Angle(currentUp, targetUp);
            
            var torque = rotationAxis.normalized * (rotationAngle * rotationTorque);
            
            var dampingTorque = -_hipsRb.angularVelocity * rotationDamping;
            
            _hipsRb.AddTorque(torque + dampingTorque, ForceMode.Force);
            
            if (showDebugRays)
            {
                Debug.DrawRay(hips.position, currentUp * 1.5f, Color.red);
                Debug.DrawRay(hips.position, targetUp * 1.5f, Color.cyan);
            }
        }

        void OnDrawGizmosSelected() {
            if (hips == null) return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hips.position - Vector3.up * (hips.position.y - desiredHipHeight), 0.2f);
        }
    }
}