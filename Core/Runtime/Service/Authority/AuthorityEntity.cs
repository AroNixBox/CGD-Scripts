using System;
using UnityEngine;

namespace Core.Runtime.Authority {
    /// <summary>
    /// Identifier Component for objects that can have authority.
    /// </summary>
    public class AuthorityEntity : MonoBehaviour {
        AuthorityManager _authorityManager;
        public void Initialize(AuthorityManager authManager) => _authorityManager = authManager;
        
        public bool HasAuthority() {
            if (_authorityManager == null) {
                Debug.LogError("AuthorityEntity not initialized properly");
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
            
            _authorityManager.GiveNextEntityAuthority(this);
        }
    }
}