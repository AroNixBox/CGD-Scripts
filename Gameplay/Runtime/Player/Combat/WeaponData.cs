using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/WeaponData", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject {
        [field: SerializeField] public Weapon Weapon { get; private set; }
        // TODO: Animation
        [field: SerializeField] public ProjectileData ProjectileData { get; private set; }
    }
}