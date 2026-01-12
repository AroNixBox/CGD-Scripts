using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class WeaponSway : MonoBehaviour {
        [Header("Rotation Sway")]
        [SerializeField] float rotationSmooth = 10f;
        [SerializeField] float rotationSwayMultiplier = 0.5f; 
        [SerializeField] float maxRotationSwayAngle = 5f;

        [Header("Position Sway")]
        [SerializeField] float positionSmooth = 10f;
        [SerializeField] float positionSwayMultiplier = 0.002f;
        [SerializeField] float maxPositionSwayAmount = 0.05f;
        
        Quaternion _initialLocalRotation;
        Vector3 _initialLocalPosition;

        void Awake() {
            _initialLocalRotation = transform.localRotation;
            _initialLocalPosition = transform.localPosition;
        }

        public void ProcessSway(Vector2 inputDelta, bool isMouse) {
            float mouseX = inputDelta.x * rotationSwayMultiplier;
            float mouseY = inputDelta.y * rotationSwayMultiplier;
            
            // Adjust for device sensitivity diffs: Mouse is pixels, Gamepad is 0-1
            if (isMouse) {
                // Tune down significantly for mouse delta
                mouseX *= 0.05f;
                mouseY *= 0.05f;
            } else {
                // Boost for gamepad (0-1) to reach perceptible angles
                mouseX *= 5f;
                mouseY *= 5f;
            }
            
            // Limit angles
            mouseX = Mathf.Clamp(mouseX, -maxRotationSwayAngle, maxRotationSwayAngle);
            mouseY = Mathf.Clamp(mouseY, -maxRotationSwayAngle, maxRotationSwayAngle);

            Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right); 
            Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

            Quaternion targetRotation = _initialLocalRotation * rotationX * rotationY;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, rotationSmooth * Time.deltaTime);
            
            // Position Sway (Opposite to movement)
            float moveX = -inputDelta.x * positionSwayMultiplier;
            float moveY = -inputDelta.y * positionSwayMultiplier;
            
            if (isMouse) {
                moveX *= 0.05f;
                moveY *= 0.05f;
            } else {
                moveX *= 5f;
                moveY *= 5f;
            }
            
            moveX = Mathf.Clamp(moveX, -maxPositionSwayAmount, maxPositionSwayAmount);
            moveY = Mathf.Clamp(moveY, -maxPositionSwayAmount, maxPositionSwayAmount);
            
            Vector3 targetPosition = _initialLocalPosition + new Vector3(moveX, moveY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, positionSmooth * Time.deltaTime);
        }
    }
}

