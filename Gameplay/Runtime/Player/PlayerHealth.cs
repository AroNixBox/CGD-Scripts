using Common.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime._Scripts.Gameplay.Runtime.Player {
    public class PlayerHealth : MonoBehaviour, IDamageable, IDestructible {
        [SerializeField] uint maxHealth = 100;
        float _currentHealth;

        void Awake() {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(float damage) {
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _currentHealth);

            if (_currentHealth <= 0) {
                Destruct();
            }
        }
        public void Destruct() {
            // Problem:
            // Bullet Auto-Gives Request to next Player after BulletCam completed
            // TODO:
            // Maybe inform some Manager, which can be requested via Bullet to not give the next one Authority.
            // Alternatively AuthorityManager can catch if this methods is executed & stop the permission to the next player..
        }
    }
}