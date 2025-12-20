using System;
using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime.Entity {
    public class EntityHealth : MonoBehaviour, IDamageable {
        [SerializeField] uint maxHealth = 100;
        float _currentHealth;
        public event Action<float> OnCurrentHealthChanged = delegate { };
        public event Action<float> OnHealthDepleted = delegate { };

        void Awake() {
            _currentHealth = maxHealth;
        }

        public float GetHealth() {
            return _currentHealth;
        }

        public void TakeDamage(float damage) {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _currentHealth);
            Debug.Log("Entity took damage: " + damage);

            if (_currentHealth <= 0) {
                OnHealthDepleted?.Invoke(0);
                Die();
            } else {
                OnCurrentHealthChanged?.Invoke(_currentHealth);
            }
        }

        void Die() {
            Destroy(gameObject);
        }
    }
}