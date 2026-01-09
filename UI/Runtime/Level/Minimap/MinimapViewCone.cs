using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level.Minimap {
    /// <summary>
    /// Represents the viewing cone of the active player on the minimap.
    /// </summary>
    public class MinimapViewCone : MonoBehaviour {
        [SerializeField, Required]
        private RectTransform rectTransform;
        
        [SerializeField, Required]
        private Image coneImage;
        
        [BoxGroup("Cone Settings")]
        [SerializeField, Range(10f, 180f)]
        private float coneAngle = 60f;
        
        [BoxGroup("Cone Settings")]
        [SerializeField]
        private float coneLength = 50f;
        
        [BoxGroup("Cone Settings")]
        [SerializeField]
        private Color coneColor = new Color(1f, 1f, 0f, 0.3f);

        private void Awake() {
            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
            
            if (coneImage != null) {
                coneImage.color = coneColor;
            }
        }

        /// <summary>
        /// Updates the position of the view cone on the minimap.
        /// </summary>
        public void UpdatePosition(Vector2 minimapPosition) {
            if (rectTransform != null) {
                rectTransform.anchoredPosition = minimapPosition;
            }
        }

        /// <summary>
        /// Updates the rotation of the view cone based on player's Y rotation.
        /// </summary>
        public void UpdateRotation(float yRotation) {
            if (rectTransform != null) {
                // Offset by half the cone angle to center the cone around the forward direction
                float halfAngleOffset = coneAngle * 0.5f;
                rectTransform.localRotation = Quaternion.Euler(0, 0, -yRotation + halfAngleOffset);
            }
        }

        /// <summary>
        /// Sets the angle of the view cone in degrees.
        /// </summary>
        public void SetConeAngle(float angle) {
            coneAngle = Mathf.Clamp(angle, 10f, 180f);
            UpdateConeVisual();
        }

        /// <summary>
        /// Sets the length of the view cone.
        /// </summary>
        public void SetConeLength(float length) {
            coneLength = length;
            UpdateConeVisual();
        }

        /// <summary>
        /// Sets the color of the view cone.
        /// </summary>
        public void SetConeColor(Color color) {
            coneColor = color;
            if (coneImage != null) {
                coneImage.color = coneColor;
            }
        }

        private void UpdateConeVisual() {
            if (rectTransform == null) return;
            
            // Scale the cone based on length
            rectTransform.sizeDelta = new Vector2(coneLength, coneLength);
            
            // The cone angle is typically handled by the sprite/shader
            // For a simple implementation, we can use fill amount if using a radial filled image
            if (coneImage != null && coneImage.type == Image.Type.Filled) {
                coneImage.fillAmount = coneAngle / 360f;
            }
        }

        private void OnValidate() {
            UpdateConeVisual();
            if (coneImage != null) {
                coneImage.color = coneColor;
            }
        }
    }
}

