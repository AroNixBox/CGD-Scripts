using Core.Runtime.Authority;
using Core.Runtime.Backend;
using Core.Runtime.Service;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarController : MonoBehaviour {
        [SerializeField, Required] HealthBarView view;
        UserData _userData;
        public UserData UserData => _userData;
        IDamageable _damageable;
        AuthorityManager _authorityManager;

        // TODO: Inject correct damageInheritor
        public void Init(IDamageable damageable, UserData userData) {
            _damageable = damageable;
            _userData = userData;
            
            view.InitializeHealthBar(damageable.GetHealth(), userData.Username, userData.UserIcon);
            _damageable.OnCurrentHealthChanged += view.UpdateHealthBar;
            if (!ServiceLocator.TryGet(out _authorityManager)) return;

            _authorityManager.OnEntityAuthorityGained += CheckAuthorityAndHighlight;
            _authorityManager.OnEntityAuthorityRevoked += CheckAuthorityAndDisableHighlight;
            // TODO: Subscribe on player turn started/ended
        }

        void CheckAuthorityAndHighlight(AuthorityEntity authEntity) {
            if (!_authorityManager.IsUserAuthorityEntity(authEntity, _userData)) return;
            view.SetNameToActiveColor();
        }
        void CheckAuthorityAndDisableHighlight(AuthorityEntity authEntity) {
            if (!_authorityManager.IsUserAuthorityEntity(authEntity, _userData)) return;
            view.SetNameToInactiveColor();
        }
        
        void OnDisable() {
            if (_damageable == null) return;
            
            _damageable.OnCurrentHealthChanged -= view.UpdateHealthBar;

            if (_authorityManager == null) return;
            _authorityManager.OnEntityAuthorityGained += CheckAuthorityAndHighlight;
            _authorityManager.OnEntityAuthorityRevoked += CheckAuthorityAndDisableHighlight;
        }
    }
}
