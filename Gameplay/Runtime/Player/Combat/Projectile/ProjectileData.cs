using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Data")]
    public class ProjectileData: ScriptableObject {
        // TODO: ShotPattern (How many are fired, etc..)
        [Required, InlineEditor] public ProjectileImpactData impactData;
        [Required] public Projectile projectilePrefab;
        [Required] public float mass = 1;
        [Required] public float drag;
    }
}