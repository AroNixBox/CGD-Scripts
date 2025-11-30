using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarController : MonoBehaviour {
        [SerializeField, Required] HealthBarView view;
        IDamageable _damageable;

        // TODO: Inject correct damageInheritor
        public void Init(IDamageable damageable) {
            _damageable = damageable;
            
            view.InitializeHealthBar(damageable.GetMaxHealth());
            _damageable.OnCurrentHealthChanged += view.UpdateHealthBar;
        }
        
        void OnDisable() {
            if (_damageable == null) return;
            
            _damageable.OnCurrentHealthChanged -= view.UpdateHealthBar;
        }
    }
}
