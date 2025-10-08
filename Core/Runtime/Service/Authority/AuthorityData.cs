using System;
using UnityEngine;

namespace Core.Runtime.Authority {
    /// <summary>
    /// Middleman that keeps track of which object currently has authority.
    /// </summary>
    [CreateAssetMenu(fileName = "AuthorityData", menuName = "Authority/Authority")]
    public class AuthorityData : ScriptableObject {
        public event Action<object> OnAuthorityChanged;
        public object Owner => _owner;
        object _owner;

        public void SetAuthority(object newOwner) {
            _owner = newOwner;
            OnAuthorityChanged?.Invoke(_owner);
            Debug.Log($"Authority set to {_owner}");
        }

        public void ResetAuthority() {
            if(_owner == null) {
                Debug.LogWarning("Attempted to reset authority when there is no owner.");
                return;
            }
            
            var oldOwner = _owner;
            _owner = null;
            OnAuthorityChanged?.Invoke(null);
            Debug.Log($"Authority reset from {oldOwner} to null");
        }

        public bool HasAuthority(object candidate) => ReferenceEquals(_owner, candidate);
    }
}