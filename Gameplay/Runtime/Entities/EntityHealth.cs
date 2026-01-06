using System;
using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime.Player {
    public class EntityHealth : MonoBehaviour, IDamageable {
        [SerializeField] uint maxHealth = 100;
        float _currentHealth;
        public event Action<float> OnCurrentHealthChanged = delegate { };

        void Awake() {
            _currentHealth = maxHealth;
        }

        public float GetHealth() {
            return _currentHealth;
        }

        public void TakeDamage(float damage) {
            if (damage <= 0) return; // Cant die again
            
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _currentHealth);
            OnCurrentHealthChanged?.Invoke(_currentHealth);
            
            // Did the player die this round?
            if (!(_currentHealth <= 0)) return;
                
            Destruct();
        }

        void Destruct() {
            // TODO: Somehow inform Entity System that we are being destroyed
            gameObject.SetActive(false);
        }
    }
}