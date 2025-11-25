using System;

namespace Gameplay.Runtime.Interfaces {
    public interface IDamageable {
        event Action<float> OnMaxHealthInitialized; // Called on init
        event Action<float> OnCurrentHealthChanged;
        public void TakeDamage(float damage);
    }
}