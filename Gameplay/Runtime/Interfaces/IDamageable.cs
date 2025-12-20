using System;

namespace Gameplay.Runtime.Interfaces {
    public interface IDamageable {
        public float GetHealth();
        event Action<float> OnCurrentHealthChanged;
        event Action<float> OnHealthDepleted;
        public void TakeDamage(float damage);
    }
}