using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level.WorldSpace {
    public class HealthBar : MonoBehaviour {
        [SerializeField, Required] Slider uiHealthBar;
        [SerializeField, Required] MonoBehaviour damageableInheritor;
        IDamageable _damageable;

        void Awake() {
            if (!damageableInheritor.TryGetComponent(out _damageable)) {
                Debug.LogError($"Assigned damageableInheritor does not implement an {damageableInheritor.GetType()}");
            }
        }

        void OnEnable() {
            _damageable.OnMaxHealthInitialized += InitializeHealthBar;
            _damageable.OnCurrentHealthChanged += UpdateHealthBar;
        }

        void UpdateHealthBar(float currentHealth) {
            uiHealthBar.value = Mathf.Clamp(currentHealth, 0, uiHealthBar.maxValue);
        }

        void InitializeHealthBar(float maxHealth) {
            uiHealthBar.maxValue = maxHealth;
            uiHealthBar.value = maxHealth;
        }
        
        void OnDisable() {
            _damageable.OnMaxHealthInitialized -= InitializeHealthBar;
            _damageable.OnCurrentHealthChanged -= UpdateHealthBar;
        }

    }
}
