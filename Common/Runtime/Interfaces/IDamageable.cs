using System;

namespace Common.Runtime.Interfaces {
    public interface IDamageable {
        event Action<float> OnCurrentHealthChanged;
        event Action OnDestroyed;
        public void TakeDamage(float damage);
    }

    public interface IDestructible {
        public void Destruct();
    }
}