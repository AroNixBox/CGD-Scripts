using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject {
        [Tooltip("Physical Weapon Prefab")]
        [field: SerializeField] public Weapon Weapon { get; private set; }
        // TODO: Specific Animation for this Weapon for Aiming & Shooting, alternatively IK
        [SerializeField, Required] public ProjectileData projectileData;
        public ProjectileData ProjectileData => projectileData;
        [SerializeField, Required] public GlobalWeaponData globalWeaponData;
        public GlobalWeaponData GlobalWeaponData => globalWeaponData;
        [SerializeField, Required] Sprite menuIcon;
        public Sprite MenuIcon => menuIcon;
    }
}