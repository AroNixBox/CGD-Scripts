using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class PlayerWeaponStash : MonoBehaviour {
        [SerializeField, Required] WeaponLoadout loadout;
        [SerializeField, Required] Transform weaponSocket;

        readonly Dictionary<string, List<(WeaponData weaponData, int ammo)>> _weaponCategoryDatasMapping = new();
        readonly Dictionary<string, int> _lastSelectedIndexPerCategory = new();
        
        WeaponData _currentWeaponData;
        Weapon _spawnedWeapon; 

        // Input control
        const float PressThreshold = 0.5f;
        bool _inputReset = true;

        // Events
        public event Action<string, WeaponData> OnWeaponDataAdded = delegate { };
        public event Action<string, WeaponData> OnWeaponDataSelected = delegate { };
        public event Action<string, WeaponData, int> OnAmmoChanged = delegate { };
        public event Action<Projectile> OnSuccessfulShot = delegate { };
        public event Action<float> OnProjectileForceChanged = delegate { };

        void Start() {
            if (loadout == null) throw new NullReferenceException("Weapon Loadout needs to be referenced");

            foreach (var entry in loadout.WeaponLoadoutEntries) {
                if (entry.WeaponData == null) continue;

                // 1. Get the category via impact strategy
                var strategy = entry.WeaponData.ProjectileData.impactData.GetImpactStrategy();
                
                string typeKey;

                // if impact strategy is aoe differentiate between damage and yeet
                // Yeet = >5dmg and some knockback
                // Else Damage
                if (strategy is AOEDamageStrategy aoeStrategy) {
                    if (aoeStrategy.MaximumDamage > 5 && aoeStrategy.MaximumExplosionForce > 0) {
                        typeKey = "Yeet";
                    }
                    else {
                        typeKey = "Damage";
                    }
                }
                else {
                    // Fallback, null strategory, map under uncategorized
                    typeKey = strategy != null 
                        ? strategy.GetType().ToString() 
                        : "Uncategorized";
                }

                // 2. Add List in Dict if no entry yet for that category
                if (!_weaponCategoryDatasMapping.ContainsKey(typeKey)) {
                    _weaponCategoryDatasMapping[typeKey] = new List<(WeaponData, int)>();
                }
                
                // 3. Add weapon and ammunition from the loadout
                _weaponCategoryDatasMapping[typeKey].Add((entry.WeaponData, entry.Ammunition));
                
                // Events
                OnWeaponDataAdded.Invoke(typeKey, entry.WeaponData);
                OnAmmoChanged.Invoke(typeKey, entry.WeaponData, entry.Ammunition);
            }

            // Set first weapon of first category
            if (_weaponCategoryDatasMapping.Count > 0) {
                 var firstGroup = _weaponCategoryDatasMapping.Values.FirstOrDefault();
                 if (firstGroup is { Count: > 0 }) {
                     _currentWeaponData = firstGroup[0].weaponData;
                 }
            }
        }

        public void SelectWeapon(Vector2 input) {
            if (_weaponCategoryDatasMapping.Count == 0) return;

            var xActive = Mathf.Abs(input.x) > PressThreshold;
            var yActive = Mathf.Abs(input.y) > PressThreshold;

            if (!xActive && !yActive) {
                _inputReset = true;
                return;
            }

            if (!_inputReset) return;

            // Snapshot <WeaponData, Ammo>
            var groups = _weaponCategoryDatasMapping.Values.ToList();

            var currentGroupIdx = -1;
            var currentWeaponIdx = -1;

            // Get index of that weapon
            if (_currentWeaponData != null) {
                for (var i = 0; i < groups.Count; i++) {
                    var groupList = groups[i];
                    // Get fitting weapon data
                    for (int j = 0; j < groupList.Count; j++) {
                        if (groupList[j].weaponData == _currentWeaponData) {
                            currentGroupIdx = i;
                            currentWeaponIdx = j;
                            goto FoundIndices; // Break both loops
                        }
                    }
                }
            }
            FoundIndices:

            // Fallback at invalid, no weapondata found, just select first.
            if (currentGroupIdx == -1) {
                currentGroupIdx = 0;
                currentWeaponIdx = 0;
            }

            WeaponData nextSelection = null;

            if (xActive) {
                // Horizontal: Switch Category
                int nextGroupIdx = input.x > 0
                    ? (currentGroupIdx + 1) % groups.Count
                    : (currentGroupIdx - 1 + groups.Count) % groups.Count;

                var nextGroup = groups[nextGroupIdx];
                var categoryKey = _weaponCategoryDatasMapping.Keys.ElementAt(nextGroupIdx);

                // Get last index for that category if there was one
                int lastIndex = _lastSelectedIndexPerCategory.GetValueOrDefault(categoryKey, 0);
                if (nextGroup.Count > 0 && lastIndex < nextGroup.Count)
                    nextSelection = nextGroup[lastIndex].weaponData;
            }
            else {
                // Vertical: Switch Gun in selected category
                var group = groups[currentGroupIdx];
                if (group.Count > 1) {
                    // Up (>0): Prev, Down (<0): Next Gun
                    var nextWeaponIdx = input.y < 0
                        ? (currentWeaponIdx - 1 + group.Count) % group.Count
                        : (currentWeaponIdx + 1) % group.Count;
                    nextSelection = group[nextWeaponIdx].weaponData;
                }
            }

            if (nextSelection != null && nextSelection != _currentWeaponData) {
                SelectWeapon(nextSelection);
                _inputReset = false;
            }
        }

        public void SelectCurrentWeapon() {
             if(_currentWeaponData != null)
                 SelectWeapon(_currentWeaponData);
        }

        void SelectWeapon(WeaponData weaponData) {
            if (weaponData == null) return;

            string foundCategory = null;

            // Find the WeaponData
            foreach (var kvp in _weaponCategoryDatasMapping) {
                if (kvp.Value.Any(x => x.weaponData == weaponData)) {
                    foundCategory = kvp.Key;
                    break;
                }
            }

            // Weapon we are trying to find is not in the Stash
            if (foundCategory == null) {
                Debug.LogError("Weapon you tried to select is not in the stash.. Shouldnt happen");
                return;
            }
            
            // Save index for that category, so we can return to it later
            var categoryList = _weaponCategoryDatasMapping[foundCategory];
            for (int i = 0; i < categoryList.Count; i++) {
                if (categoryList[i].weaponData == weaponData) {
                    _lastSelectedIndexPerCategory[foundCategory] = i;
                    break;
                }
            }
            

            DespawnSelectedWeapon();
            
            _currentWeaponData = weaponData;
            OnWeaponDataSelected.Invoke(foundCategory, _currentWeaponData);
            
            SpawnSelectedWeapon();
        }


        void SpawnSelectedWeapon() {
            if (_currentWeaponData == null) return;
            var currentWeaponPrefab = _currentWeaponData.Weapon;
            _spawnedWeapon = Instantiate(currentWeaponPrefab, weaponSocket);
            _spawnedWeapon.Init(_currentWeaponData);
            _spawnedWeapon.OnProjectileForceChanged += HandleProjectileForceChanged;
            
            // Notify initial value
            OnProjectileForceChanged.Invoke(_spawnedWeapon.ProjectileForcePercent);
        }
        
        void HandleProjectileForceChanged(float percent) {
            OnProjectileForceChanged.Invoke(percent);
        }

        public WeaponData GetCurrentWeaponData() => _currentWeaponData;

        public bool TryFire(Action<bool> onProjectileExpired, out Projectile projectile) {
            projectile = null;
            var weapon = GetSpawnedWeapon();
            if (weapon == null) {
                Debug.LogError("Cannot fire: No weapon spawned.");
                return false;
            }

            // Get fitting tuple to find ammunition
            string targetCategory = null;
            List<(WeaponData weaponData, int ammo)> targetList = null;
            int targetIndex = -1;

            // Get Category
            foreach (var kvp in _weaponCategoryDatasMapping) {
                var list = kvp.Value;
                for (int i = 0; i < list.Count; i++) {
                    if (list[i].weaponData == _currentWeaponData) {
                        targetCategory = kvp.Key;
                        targetList = list;
                        targetIndex = i;
                        break;
                    }
                }
                if (targetList != null) break;
            }

            // Invalid
            if (targetList == null || targetIndex == -1 || targetCategory == null) return false;

            var entry = targetList[targetIndex];
            if (entry.ammo <= 0) return false;

            // decrease local ammo variable and override the tuple with the ammo
            entry.ammo--;
            targetList[targetIndex] = entry;

            projectile = weapon.FireWeapon(onProjectileExpired);
            
            OnSuccessfulShot?.Invoke(projectile);
            OnAmmoChanged?.Invoke(targetCategory, entry.weaponData, entry.ammo);
            return true;
        }


        public Weapon GetSpawnedWeapon() => _spawnedWeapon;

        public void DespawnSelectedWeapon() {
            if(_spawnedWeapon != null) {
                _spawnedWeapon.OnProjectileForceChanged -= HandleProjectileForceChanged;
                _spawnedWeapon.Dispose();
            }
        }
    }
}
