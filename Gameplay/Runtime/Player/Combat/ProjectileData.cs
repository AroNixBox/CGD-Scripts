using Gameplay.Runtime.Player.Trajectory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Data")]
    public class ProjectileData: ScriptableObject {
        // TODO: ShotPattern (How many are fired, etc..)
        [field: SerializeField][Required] public ProjectileImpactData ImpactData { get; private set; }
        [field: SerializeField][Required] public Projectile ProjectilePrefab { get; private set; }
        [field: SerializeField] [Required] public float Mass { get; private set; } = 1;
        [field: SerializeField][Required] public float Drag { get; private set; }
    }
}