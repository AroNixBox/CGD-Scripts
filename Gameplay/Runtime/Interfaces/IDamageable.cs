using System;

namespace Gameplay.Runtime.Interfaces {
    public interface IDamageable {
        public uint GetMaxHealth();
        event Action<float> OnCurrentHealthChanged;
        public void TakeDamage(float damage);
    }
}