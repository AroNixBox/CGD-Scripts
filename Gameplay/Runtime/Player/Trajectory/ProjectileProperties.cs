using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    public struct ProjectileProperties {
        public Vector3 direction;
        public Vector3 initialPosition;
        public readonly float initialSpeed;
        public readonly float mass;
        public readonly float drag;
        
        public ProjectileProperties(Vector3 direction, Vector3 initialPosition, float initialSpeed, float mass, float drag) {
            this.direction = direction;
            this.initialPosition = initialPosition;
            this.initialSpeed = initialSpeed;
            this.mass = mass;
            this.drag = drag;
        }
    }
}