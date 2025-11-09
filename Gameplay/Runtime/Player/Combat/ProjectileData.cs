using Gameplay.Runtime.Player.Trajectory;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/ProjectileData", fileName = "ProjectileData")]
    public class ProjectileData: ScriptableObject {
        // TODO: ShotPattern (How many are fired, etc..
        [field: SerializeField] public float ThrowForce { get; private set; }
        [field: SerializeField] public Rigidbody Projectile { get; private set; }
        
        public ProjectileProperties GetProjectileProperties() {
            // TODO: Cache
            return new ProjectileProperties(
                ThrowForce, 
                Projectile.mass,
                Projectile.linearDamping
            );
        }
    }
}