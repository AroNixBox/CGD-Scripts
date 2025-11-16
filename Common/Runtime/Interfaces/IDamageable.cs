namespace Common.Runtime.Interfaces {
    public interface IDamageable {
        public void TakeDamage(float damage);
    }

    public interface IDestructible {
        public void Destruct();
    }
}