using System.Collections.Generic;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Gameplay.Runtime.Player;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level.Minimap {
    /// <summary>
    /// Manages player icons and view cone on the minimap.
    /// The minimap camera should be pre-configured in the scene.
    /// </summary>
    public class MinimapController : MonoBehaviour {
        [BoxGroup("Camera")]
        [SerializeField, Required, Tooltip("Reference to the pre-configured minimap camera")] 
        private Camera minimapCamera;
        
        [BoxGroup("Icons")]
        [SerializeField, Required]
        private MinimapIcon iconPrefab;
        
        [BoxGroup("Icons")]
        [SerializeField, Required]
        private RectTransform iconContainer;
        
        [BoxGroup("Icons")]
        [SerializeField, Required]
        private RectTransform minimapRect;
        
        [BoxGroup("View Cone")]
        [SerializeField, Required]
        private MinimapViewCone viewConePrefab;
        
        private readonly Dictionary<AuthorityEntity, MinimapIcon> _playerIcons = new();
        private readonly Dictionary<AuthorityEntity, PlayerController> _playerControllers = new();
        private MinimapViewCone _activeViewCone;
        private AuthorityManager _authorityManager;
        private AuthorityEntity _currentActiveEntity;
        private PlayerController _currentActivePlayerController;

        private void Start() {
            if (ServiceLocator.TryGet(out _authorityManager)) {
                _authorityManager.OnEntityAuthorityGained += HandleAuthorityGained;
                _authorityManager.OnEntityAuthorityRevoked += HandleAuthorityRevoked;
            }
            
            // Subscribe to entity spawn events
            AuthorityManager.OnEntitySpawned += HandleEntitySpawned;
            AuthorityManager.OnEntityDied += HandleEntityDied;
            
            // Create view cone instance
            _activeViewCone = Instantiate(viewConePrefab, iconContainer);
            _activeViewCone.gameObject.SetActive(false);
        }

        private void OnDestroy() {
            if (_authorityManager != null) {
                _authorityManager.OnEntityAuthorityGained -= HandleAuthorityGained;
                _authorityManager.OnEntityAuthorityRevoked -= HandleAuthorityRevoked;
            }
            
            AuthorityManager.OnEntitySpawned -= HandleEntitySpawned;
            AuthorityManager.OnEntityDied -= HandleEntityDied;
        }

        private void LateUpdate() {
            UpdateIconPositions();
            UpdateViewCone();
        }


        private void HandleEntitySpawned(AuthorityEntity entity) {
            if (_playerIcons.ContainsKey(entity)) return;
            
            var icon = Instantiate(iconPrefab, iconContainer);
            icon.Initialize(entity);
            _playerIcons.Add(entity, icon);
            
            // Cache PlayerController reference
            if (entity.TryGetComponent(out PlayerController playerController)) {
                _playerControllers.Add(entity, playerController);
            }
        }

        private void HandleEntityDied(AuthorityEntity entity) {
            if (!_playerIcons.TryGetValue(entity, out var icon)) return;
            
            _playerIcons.Remove(entity);
            _playerControllers.Remove(entity);
            
            if (icon != null) {
                Destroy(icon.gameObject);
            }
            
            if (_currentActiveEntity == entity) {
                _currentActiveEntity = null;
                _currentActivePlayerController = null;
                _activeViewCone.gameObject.SetActive(false);
            }
        }

        private void HandleAuthorityGained(AuthorityEntity entity) {
            _currentActiveEntity = entity;
            
            // Update icon states
            foreach (var kvp in _playerIcons) {
                kvp.Value.SetActive(kvp.Key == entity);
            }
            
            // Cache PlayerController reference
            if (_playerControllers.TryGetValue(entity, out var playerController)) {
                _currentActivePlayerController = playerController;
            }
            
            // Show view cone immediately
            _activeViewCone.gameObject.SetActive(true);
        }

        private void HandleAuthorityRevoked(AuthorityEntity entity) {
            if (_currentActiveEntity == entity) {
                _currentActiveEntity = null;
                _currentActivePlayerController = null;
                _activeViewCone.gameObject.SetActive(false);
                
                foreach (var kvp in _playerIcons) {
                    kvp.Value.SetActive(false);
                }
            }
        }
        

        private void UpdateIconPositions() {
            foreach (var kvp in _playerIcons) {
                var entity = kvp.Key;
                var icon = kvp.Value;
                
                if (entity == null || icon == null) continue;
                
                var worldPos = entity.transform.position;
                var minimapPos = WorldToMinimapPosition(worldPos);
                icon.UpdatePosition(minimapPos);
            }
        }

        private void UpdateViewCone() {
            if (_currentActiveEntity == null || _activeViewCone == null) return;
            
            var worldPos = _currentActiveEntity.transform.position;
            var minimapPos = WorldToMinimapPosition(worldPos);
            _activeViewCone.UpdatePosition(minimapPos);
            
            // Get aim direction from camera when in combat stance
            float yRotation;
            if (_currentActivePlayerController != null) {
                var cameraTransform = _currentActivePlayerController.PlayerCameraControls.GetActiveCameraTransform();
                // Project camera forward onto XZ plane to get horizontal aim direction
                var cameraForward = cameraTransform.forward;
                var horizontalForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
                yRotation = Mathf.Atan2(horizontalForward.x, horizontalForward.z) * Mathf.Rad2Deg;
            } else {
                yRotation = _currentActiveEntity.transform.eulerAngles.y;
            }
            
            _activeViewCone.UpdateRotation(yRotation);
        }

        private Vector2 WorldToMinimapPosition(Vector3 worldPosition) {
            if (minimapCamera == null || minimapRect == null) return Vector2.zero;
            
            // Get viewport position from minimap camera
            var viewportPos = minimapCamera.WorldToViewportPoint(worldPosition);
            
            // Convert to minimap rect local position
            var minimapSize = minimapRect.rect.size;
            var localPos = new Vector2(
                (viewportPos.x - 0.5f) * minimapSize.x,
                (viewportPos.y - 0.5f) * minimapSize.y
            );
            
            return localPos;
        }
    }
}

