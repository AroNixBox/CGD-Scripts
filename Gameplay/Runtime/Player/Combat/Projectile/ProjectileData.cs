using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Data")]
    public class ProjectileData: ScriptableObject {
        // TODO: ShotPattern (How many are fired, etc..)
        [Required] public ProjectileImpactData impactData;
        [Required] public Projectile projectilePrefab;
        [Required] public float mass = 1;
        [Required] public float drag;

        [SerializeField] GameObject muzzleEffect;
        public GameObject MuzzleEffect => muzzleEffect;
        [SerializeField] AudioClip firedSound;
        public AudioClip FiredSound => firedSound;
    }
}