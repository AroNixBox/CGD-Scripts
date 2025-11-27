using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level.WorldSpace {
    public class HealthBarController : MonoBehaviour {
        [SerializeField, Required] HealthBarView view;
        [SerializeField, Required] MonoBehaviour damageableInheritor;
        IDamageable _damageable;

        void Awake() {
            if (!damageableInheritor.TryGetComponent(out _damageable)) {
                Debug.LogError($"Assigned damageableInheritor does not implement an {damageableInheritor.GetType()}");
            }
        }

        void OnEnable() {
            _damageable.OnMaxHealthInitialized += view.InitializeHealthBar;
            _damageable.OnCurrentHealthChanged += view.UpdateHealthBar;
        }
        
        void OnDisable() {
            _damageable.OnMaxHealthInitialized -= view.InitializeHealthBar;
            _damageable.OnCurrentHealthChanged -= view.UpdateHealthBar;
        }
    }
}
