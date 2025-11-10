using Gameplay.Runtime.Player.Trajectory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Data")]
    public class ProjectileData: ScriptableObject {
        // TODO: ShotPattern (How many are fired, etc..)
        [field: SerializeField][Required] public ProjectileImpactData ImpactData { get; private set; }
        // TODO: ThrowForce should be able to be adjusted by Player
        [field: SerializeField][Required] public Rigidbody ProjectilePrefab { get; private set; }
    }
}