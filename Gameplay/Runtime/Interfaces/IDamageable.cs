using System;

namespace Gameplay.Runtime.Interfaces {
    public interface IDamageable {
        public float GetHealth();
        event Action<float> OnCurrentHealthChanged;
        public void TakeDamage(float damage);
    }
}