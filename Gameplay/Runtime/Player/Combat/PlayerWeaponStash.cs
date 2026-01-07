using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class PlayerWeaponStash : MonoBehaviour {
        [SerializeField, Required] WeaponLoadout loadout;
        readonly Dictionary<WeaponData, int> _runtimeLoadout = new();
        
        // TODO: We Could have 1 WeaponSocket per weapon
        [SerializeField, Required] Transform weaponSocket;
        int _currentWeaponIndex;

        Weapon _spawnedWeapon;
        
        // Input control:
        const float PressThreshold = 0.5f;
        bool _inputReset = true;
        
        public event Action<WeaponData> OnWeaponDataAdded = delegate { };
        public event Action<WeaponData> OnWeaponDataSelected = delegate { };
        public event Action<WeaponData, int> OnAmmoChanged = delegate { }; // Weapon | Amount
        
        WeaponData[] Weapons => _runtimeLoadout.Keys.ToArray();

        void Awake() {
            if (loadout == null) throw new NullReferenceException("Weapon Loadout neets to be referenced");
            var weaponLoadoutEntries = loadout.WeaponLoadoutEntries;
            if (weaponLoadoutEntries == null || weaponLoadoutEntries.Length < 1)
                throw new NullReferenceException("Weapon Loadout Entries cant be empty");
            
            foreach (var loadoutEntry in loadout.WeaponLoadoutEntries) {
                _runtimeLoadout.TryAdd(loadoutEntry.WeaponData, loadoutEntry.Ammunition);
            }
        }

        // Init UI
        void Start() {
            // Tell other systems bout all the Weapons the player has in its layout
            if (loadout == null) throw new NullReferenceException("Weapon Loadout neets to be referenced");
            foreach (var loadoutWeaponLoadoutEntry in _runtimeLoadout) {
                var weaponData = loadoutWeaponLoadoutEntry.Key;
                if(weaponData == null) 
                    throw new NullReferenceException("Weapon Data needs to be referenced in Loadout-Entry");

                OnWeaponDataAdded.Invoke(weaponData);
                OnAmmoChanged.Invoke(weaponData, loadoutWeaponLoadoutEntry.Value);
            }
        }
        // TODO: Umbauen auf eine Methode die generell alle Vector2 horizontal und vertikal auffängt und dann differentiated
        public void SelectWeapon(Vector2 input) {
            if (input.x > PressThreshold) {
                if (_inputReset) {
                    SelectNextWeapon();
                    _inputReset = false;
                }
            }
            else if (input.x < -PressThreshold) {
                if (_inputReset) {
                    SelectPreviousWeapon();
                    _inputReset = false;
                }
            }
            else {
                // Input ist fast 0, Reset für nächsten Druck erlauben
                _inputReset = true;
            }
        }
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
            if (Weapons.Length is 0 or 1) // No Scroll needed
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
        
        int GetNextIndex() {
            if(Weapons.Length == 0)
                throw new UnassignedReferenceException("Weapons Array not assigned");
            
            return _currentWeaponIndex < Weapons.Length - 1
                ? _currentWeaponIndex + 1
                : 0;
        }

        int GetPreviousIndex() =>
            _currentWeaponIndex == 0
                ? Weapons.Length - 1
                : _currentWeaponIndex - 1;

        // Use for static Data-Information
        WeaponData GetCurrentWeaponData() {
            if(Weapons.Length == 0)
                throw new UnassignedReferenceException("Weapons Array not assigned");
            
            return Weapons[_currentWeaponIndex];
        }
        
        // In WeaponStash
        public bool TryFire(Action<bool> onProjectileExpired, out Projectile projectile) {
            projectile = null;
            var weapon = GetSpawnedWeapon();
            if (weapon == null) {
                Debug.LogError("Cannot fire: No weapon is currently spawned. Ensure a weapon is selected before attempting to fire.");
                return false;
            }
        
            var weaponData = GetCurrentWeaponData();
            if (!_runtimeLoadout.TryGetValue(weaponData, out var ammo) || ammo <= 0) {
                // No Ammo
                return false;
            }
        
            _runtimeLoadout[weaponData]--;
            projectile = weapon.FireWeapon(onProjectileExpired);
            // TODO:
            // Inform UI
            OnAmmoChanged?.Invoke(weaponData, _runtimeLoadout[weaponData]);
            return true;
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
