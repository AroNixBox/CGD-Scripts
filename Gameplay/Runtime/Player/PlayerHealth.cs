using System;
using Core.Runtime.Authority;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player {
    public class PlayerHealth : MonoBehaviour, IDamageable {
        [SerializeField, Required] AuthorityEntity authorityEntity;
        [SerializeField] uint maxHealth = 100;
        [SerializeField] AudioClip damageSound;
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
            PlayDamageSound(damageSound);

            if (_currentHealth <= 0) {
                Die();
            }
        }

        void PlayDamageSound(AudioClip clip) {
            if (clip == null)
                return;

            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        void Die() {
            authorityEntity.Unregister();
            Destroy(gameObject);
        }
    }
}