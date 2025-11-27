using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class PlayerWeaponStash : MonoBehaviour {
        [SerializeField, Required] WeaponData[] weapons;
        // TODO: We Could have 1 WeaponSocket per weapon and that could be somehow mapped off runtime and identified in runtime
        [SerializeField, Required] Transform weaponSocket;
        int _currentWeaponIndex;

        Weapon _spawnedWeapon;

        public event Action<WeaponData> OnWeaponDataAdded = delegate { };
        // Index wise, b
        public event Action<WeaponData> OnWeaponDataSelected = delegate { };

        // Init UI
        void Start() => weapons.ForEach(w => OnWeaponDataAdded.Invoke(w));

        public void SelectNextWeapon() {
            var nextIndex = GetNextIndex();
            SelectWeapon(nextIndex);
        }
        public void SelectPreviousWeapon() {
            var previousIndex = GetPreviousIndex();
            SelectWeapon(previousIndex);
        }
        // "Respawn" the weapon which is still selected
        public void SelectCurrentWeapon() => SelectWeapon(_currentWeaponIndex);
        void SelectWeapon(int index) {
            if (weapons.Length is 0 or 1)
                return;
            
            DespawnSelectedWeapon();
            _currentWeaponIndex = index;
            OnWeaponDataSelected.Invoke(GetCurrentWeaponData());
            SpawnSelectedWeapon();
        }
        void SpawnSelectedWeapon() {
            var currentWeaponPrefab = GetCurrentWeaponData().Weapon;
            _spawnedWeapon = Instantiate(currentWeaponPrefab, weaponSocket);
            _spawnedWeapon.Init(GetCurrentWeaponData());
        }
        int GetNextIndex() =>
            _currentWeaponIndex < weapons.Length - 1
                ? _currentWeaponIndex + 1
                : 0;

        int GetPreviousIndex() =>
            _currentWeaponIndex == 0
                ? weapons.Length - 1
                : _currentWeaponIndex - 1;

        // Use for static Data-Information
        WeaponData GetCurrentWeaponData() {
            if(weapons.Length == 0)
                throw new UnassignedReferenceException("Weapons Array not assigned");
            
            return weapons[_currentWeaponIndex];
        }

        // Use for runtime Information like Transforms that move
        public Weapon GetSpawnedWeapon() {
            return _spawnedWeapon;
        }

        public void DespawnSelectedWeapon() {
            if(_spawnedWeapon != null)
                _spawnedWeapon.Dispose();
        }
    }
}
