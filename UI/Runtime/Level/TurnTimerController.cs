using Core.Runtime.Authority;
using Core.Runtime.Service;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    /// <summary>
    ///     Controller that connects the AuthorityManager's turn timer to the TurnTimerView.
    ///     Attach this to the Player prefab alongside AuthorityEntity.
    /// </summary>
    public class TurnTimerController : MonoBehaviour {
        [SerializeField, Required]
        private TurnTimerView view;
        [SerializeField, Tooltip("If true, timer UI is hidden between turns")]
        private bool hideTimerBetweenTurns = true;
        
        private AuthorityManager _authorityManager;
        private AuthorityEntity _authorityEntity;

        private void Awake() {
            _authorityEntity = gameObject.GetComponentInParent<AuthorityEntity>();
        }

        private void Start() {
            view.SetVisible(false);
            
            _authorityManager = ServiceLocator.TryGet(out AuthorityManager manager) ? manager : null;
            if (_authorityManager == null) {
                Debug.LogError("TurnTimerController: AuthorityManager not found in ServiceLocator");
                return;
            }

            _authorityManager.OnTurnTimerUpdated += HandleTimerUpdated;
            _authorityManager.OnTurnTimerExpired += HandleTimerExpired;
            _authorityManager.OnEntityAuthorityGained += HandleAuthorityGained;
            _authorityManager.OnEntityAuthorityRevoked += HandleAuthorityRevoked;
            
            // Check if this entity already has authority (in case we subscribed late)
            if (_authorityEntity == null || !_authorityEntity.HasAuthority()) return;
            view.Initialize(_authorityManager.TurnDuration);
            view.UpdateDisplay(_authorityManager.CurrentTurnTime);
            view.SetVisible(true);
        }

        private void OnDisable() {
            if (_authorityManager == null) return;

            _authorityManager.OnTurnTimerUpdated -= HandleTimerUpdated;
            _authorityManager.OnTurnTimerExpired -= HandleTimerExpired;
            _authorityManager.OnEntityAuthorityGained -= HandleAuthorityGained;
            _authorityManager.OnEntityAuthorityRevoked -= HandleAuthorityRevoked;
        }

        private void HandleTimerUpdated(float remainingTime) {
            // Only update if this entity has authority
            if (_authorityEntity == null || !_authorityEntity.HasAuthority()) return;
            view.UpdateDisplay(remainingTime);
        }

        private void HandleTimerExpired() {
            if (_authorityEntity == null || !_authorityEntity.HasAuthority()) return;
            view.OnTimerExpired();
            if (hideTimerBetweenTurns) {
                view.SetVisible(false);
            }
        }

        private void HandleAuthorityGained(AuthorityEntity entity) {
            // Only show timer if this is our entity
            if (entity != _authorityEntity) return;
            view.Initialize(_authorityManager.TurnDuration);
            view.SetVisible(true);
        }

        private void HandleAuthorityRevoked(AuthorityEntity entity) {
            // Only hide timer if this is our entity
            if (entity != _authorityEntity) return;
            if (hideTimerBetweenTurns) {
                view.SetVisible(false);
            }
        }
    }
}