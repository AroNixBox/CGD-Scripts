using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject {
        [Tooltip("Physical Weapon Prefab")]
        [field: SerializeField] public Weapon Weapon { get; private set; }
        // TODO: Specific Animation for this Weapon for Aiming & Shooting, alternatively IK
        [field: SerializeField] public ProjectileData ProjectileData { get; private set; }
        [field: SerializeField] public int MinProjectileForce { get; private set; } = 0;
        [field: SerializeField] public int MaxProjectileForce { get; private set; } = 100;
        [Tooltip("How fast does the projectile force with player inputs")] 
        [field: SerializeField] public int ProjectileForceChangeMultiplier { get; private set; } = 100;
    }
}