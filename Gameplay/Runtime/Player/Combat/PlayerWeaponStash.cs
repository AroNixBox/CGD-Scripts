using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class PlayerWeaponStash : MonoBehaviour {
        [SerializeField, Required] WeaponData[] weapons;
        // TODO: We Could have 1 WeaponSocket per weapon and that could be somehow mapped off runtime and identified in runtime
        [SerializeField, Required] Transform weaponSocket;
        int _currentWeaponIndex;

        Weapon _spawnedWeapon;

        public void SelectNextWeapon() {
            if (weapons.Length is 0 or 1)
                return;
            
            DespawnSelectedWeapon();
            var nextIndex = GetNextIndex();
            _currentWeaponIndex = nextIndex;
            SpawnSelectedWeapon();
        }
        public void SelectPreviousWeapon() {
            if (weapons.Length is 0 or 1)
                return;
            
            DespawnSelectedWeapon();
            var previousIndex = GetPreviousIndex();
            _currentWeaponIndex = previousIndex;
            SpawnSelectedWeapon();
        }
        int GetNextIndex() =>
            _currentWeaponIndex < weapons.Length - 1
                ? _currentWeaponIndex + 1
                : 0;

        int GetPreviousIndex() =>
            _currentWeaponIndex == 0
                ? weapons.Length - 1
                : _currentWeaponIndex - 1;

        public enum EWeaponIndex {
            Next,
            Previous
        }

        // Use for static Data-Information
        public WeaponData GetCurrentWeaponData() {
            if(weapons.Length == 0)
                throw new UnassignedReferenceException("Weapons Array not assigned");
            
            return weapons[_currentWeaponIndex];
        }

        // Use for runtime Information like Transforms that move
        public Weapon GetSpawnedWeapon() {
            return _spawnedWeapon;
        }

        public void SpawnSelectedWeapon() {
            var currentWeaponPrefab = GetCurrentWeaponData().Weapon;
            _spawnedWeapon = Instantiate(currentWeaponPrefab, weaponSocket);
            _spawnedWeapon.Init(GetCurrentWeaponData());
        }

        public void DespawnSelectedWeapon() => _spawnedWeapon.Dispose();
    }
}
