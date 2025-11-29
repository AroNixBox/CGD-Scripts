using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Weapon Loadout")]
    public class WeaponLoadout : ScriptableObject {
        [SerializeField, Required] WeaponLoadoutEntry[] weaponLoadoutEntries;
        public WeaponLoadoutEntry[] WeaponLoadoutEntries => weaponLoadoutEntries;
        [System.Serializable]
        public class WeaponLoadoutEntry {
            [ValidateInput(nameof(ValidateUniqueWeapon), "Duplicate WeaponsDatas are not allowed</b>")]
            [SerializeField, Required] WeaponData weaponData;
            public WeaponData WeaponData => weaponData;
            [SerializeField] int ammunition;
            public int Ammunition => ammunition;
            
            bool ValidateUniqueWeapon(WeaponData data) {
                if (data == null) return true;

                var loadout = UnityEditor.Selection.activeObject as WeaponLoadout;
                if (loadout == null || loadout.weaponLoadoutEntries == null) return true;

                int count = loadout.weaponLoadoutEntries.Count(e => e?.weaponData == data);
                return count <= 1;
            }
        }
    }

}