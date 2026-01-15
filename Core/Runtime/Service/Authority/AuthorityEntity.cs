using System;
using Core.Runtime.Backend;
using UnityEngine;

namespace Core.Runtime.Authority {
    /// <summary>
    /// Identifier Component for objects that can have authority.
    /// </summary>
    public class AuthorityEntity : MonoBehaviour {
        AuthorityManager _authorityManager;
        public UserData UserData => _userData;
        UserData _userData;

        public event Action OnSpawned = delegate { };

        public void Initialize(AuthorityManager authManager, UserData userData) {
            _authorityManager = authManager;
            _userData = userData;
            OnSpawned?.Invoke();
        }
        
        public bool HasAuthority() {
            if (_authorityManager == null) {
                // Awaiting Init
                // Debug.LogError("AuthorityEntity not initialized properly");
                return false;
            }
            
            return _authorityManager.HasAuthority(this);
        }

        public void ResetAuthority() {
            if (_authorityManager == null) {
                Debug.LogError("AuthorityEntity not initialized properly");
                return;
            }
            
            _authorityManager.ResetAuthority(this);
        }
        
        public void GiveNextAuthority() {
            if (_authorityManager == null) {
                Debug.LogError("AuthorityEntity not initialized properly");
                return;
            }
            
            _authorityManager.GiveNextEntityAuthority();
        }

        /// <summary>
        /// Unregisters this entity from the authority system.
        /// Call this when the entity dies (e.g., from EntityHealth).
        /// </summary>
        public void Unregister() {
            if (_authorityManager == null) {
                Debug.LogError("Auth Manager is null");
                return;
            }
            
            _authorityManager.UnregisterEntity(this);
        }
    }
}