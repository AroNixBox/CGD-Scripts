namespace Gameplay.Runtime.Player.Trajectory {
    public struct ProjectileProperties {
        public float InitialSpeed { get; }
        public float Mass { get; }
        public float Drag { get; }
        
        public ProjectileProperties(float initialSpeed, float mass, float drag) {
            InitialSpeed = initialSpeed;
            Mass = mass;
            Drag = drag;
        }
    }
}