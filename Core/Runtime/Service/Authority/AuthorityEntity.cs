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

        public void Initialize(AuthorityManager authManager, UserData userData) {
            _authorityManager = authManager;
            _userData = userData;
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

        public void Unregister() {
            if (_authorityManager == null) {
                Debug.LogError("AuthorityEntity not initialized properly");
                return;
            }
            
            _authorityManager.UnregisterEntity(this);
        }
    }
}