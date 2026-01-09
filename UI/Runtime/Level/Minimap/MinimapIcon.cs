using Core.Runtime.Authority;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level.Minimap {
    /// <summary>
    /// Represents a player icon on the minimap.
    /// </summary>
    public class MinimapIcon : MonoBehaviour {
        [SerializeField, Required]
        private RectTransform rectTransform;
        
        [SerializeField, Required]
        private Image iconImage;
        
        [BoxGroup("Colors")]
        [SerializeField]
        private Color activeColor = Color.green;
        
        [BoxGroup("Colors")]
        [SerializeField]
        private Color inactiveColor = Color.gray;
        
        private AuthorityEntity _entity;

        private void Awake() {
            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
        }

        /// <summary>
        /// Initializes the icon with the player entity.
        /// </summary>
        public void Initialize(AuthorityEntity entity) {
            _entity = entity;
            
            // Set player icon if available
            if (entity.UserData.UserIcon != null) {
                iconImage.sprite = entity.UserData.UserIcon;
            }
            
            SetActive(false);
        }

        /// <summary>
        /// Updates the position of the icon on the minimap.
        /// </summary>
        public void UpdatePosition(Vector2 minimapPosition) {
            if (rectTransform != null) {
                rectTransform.anchoredPosition = minimapPosition;
            }
        }

        /// <summary>
        /// Sets whether this icon represents the active player.
        /// </summary>
        public void SetActive(bool isActive) {
            iconImage.color = isActive ? activeColor : inactiveColor;
        }

        /// <summary>
        /// Gets the associated entity.
        /// </summary>
        public AuthorityEntity Entity => _entity;
    }
}

