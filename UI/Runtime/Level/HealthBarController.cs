using Core.Runtime.Backend;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarController : MonoBehaviour {
        [SerializeField, Required] HealthBarView view;
        UserData _userData;
        public UserData UserData => _userData;
        IDamageable _damageable;

        // TODO: Inject correct damageInheritor
        public void Init(IDamageable damageable, UserData userData) {
            _damageable = damageable;
            _userData = userData;
            
            view.InitializeHealthBar(damageable.GetHealth(), userData.Username, userData.UserIcon);
            _damageable.OnCurrentHealthChanged += view.UpdateHealthBar;
        }
        
        void OnDisable() {
            if (_damageable == null) return;
            
            _damageable.OnCurrentHealthChanged -= view.UpdateHealthBar;
        }
    }
}
