using System;
using Core.Runtime.Authority;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player {
    public class PlayerHealth : MonoBehaviour, IDamageable {
        [SerializeField, Required] AuthorityEntity authorityEntity;
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
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _currentHealth);
            OnCurrentHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0) {
                Die();
            }
        }
        void Die() {
            authorityEntity.Unregister();
            Destroy(gameObject);
        }
    }
}