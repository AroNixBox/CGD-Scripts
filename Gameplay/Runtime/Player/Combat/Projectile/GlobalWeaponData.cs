using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Global Weapon Settings")]
    public class GlobalWeaponData : ScriptableObject {
        [field: SerializeField] public int MinProjectileForce { get; private set; } = 0;
        [field: SerializeField] public int MaxProjectileForce { get; private set; } = 100;
        [Tooltip("How fast does the projectile force with player inputs")] 
        [field: SerializeField] public int ProjectileForceChangeMultiplier { get; private set; } = 100;
    }
}