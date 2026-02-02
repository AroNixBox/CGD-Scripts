using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class PlayerWeaponStash : MonoBehaviour {
        [SerializeField, Required] WeaponLoadout loadout;
        [SerializeField, Required] Transform weaponSocket;

        readonly Dictionary<ProjectileData.ProjectileCategory, List<(WeaponData weaponData, int ammo)>> _weaponCategoryDatasMapping = new();
        readonly Dictionary<ProjectileData.ProjectileCategory, int> _lastSelectedIndexPerCategory = new();
        
        WeaponData _currentWeaponData;
        Weapon _spawnedWeapon; 

        // Input control
        const float PressThreshold = 0.5f;
        bool _inputReset = true;
        
        // Shooting Power:
        const float StartProjectileForce = 15;
        float _projectileForce;
        float _lastProjectileForce;

        // Events
        public event Action<ProjectileData.ProjectileCategory, WeaponData> OnWeaponDataAdded = delegate { };
        public event Action<ProjectileData.ProjectileCategory, WeaponData, int> OnWeaponDataSelected = delegate { };
        public event Action<ProjectileData.ProjectileCategory, WeaponData, int> OnAmmoChanged = delegate { };
        public event Action<Projectile> OnSuccessfulShot = delegate { };
        public event Action<float> OnProjectileForceChanged = delegate { };
        void Start() {
            if (loadout == null) throw new NullReferenceException("Weapon Loadout needs to be referenced");

            foreach (var entry in loadout.WeaponLoadoutEntries) {
                if (entry.WeaponData == null) continue;
                
                var category = entry.WeaponData.ProjectileData.Category;

                // 2. Add List in Dict if no entry yet for that category
                if (!_weaponCategoryDatasMapping.ContainsKey(category)) {
                    _weaponCategoryDatasMapping[category] = new List<(WeaponData, int)>();
                }
                
                // 3. Add weapon and ammunition from the loadout
                _weaponCategoryDatasMapping[category].Add((entry.WeaponData, entry.Ammunition));
                
                // Events
                OnWeaponDataAdded?.Invoke(category, entry.WeaponData);
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

            ProjectileData.ProjectileCategory? foundCategory = null;

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
            var categoryList = _weaponCategoryDatasMapping[foundCategory.Value];
            int currentAmmo = 0;
            for (int i = 0; i < categoryList.Count; i++) {
                if (categoryList[i].weaponData == weaponData) {
                    _lastSelectedIndexPerCategory[foundCategory.Value] = i;
                    currentAmmo = categoryList[i].ammo;
                    break;
                }
            }

            DespawnSelectedWeapon();
            
            _currentWeaponData = weaponData;
            OnWeaponDataSelected.Invoke(foundCategory.Value, _currentWeaponData, currentAmmo);
            
            SpawnSelectedWeapon();
        }

        void SpawnSelectedWeapon() {
            if (_currentWeaponData == null) return;
            var currentWeaponPrefab = _currentWeaponData.Weapon;
            
            _spawnedWeapon = Instantiate(currentWeaponPrefab, weaponSocket);
            _spawnedWeapon.Init(_currentWeaponData);
        }

        public WeaponData GetCurrentWeaponData() => _currentWeaponData;

        public void DecreaseProjectileForce() {
            if (_currentWeaponData == null) return;
            
            _projectileForce -= Time.deltaTime * _currentWeaponData.GlobalWeaponData.ProjectileForceChangeMultiplier;
            _projectileForce = Mathf.Clamp(_projectileForce, _currentWeaponData.GlobalWeaponData.MinProjectileForce, _currentWeaponData.GlobalWeaponData.MaxProjectileForce);

            NotifyProjectileForceChange();
        }

        public void IncreaseProjectileForce() {
            if (_currentWeaponData == null) return;
            
            _projectileForce += Time.deltaTime * _currentWeaponData.GlobalWeaponData.ProjectileForceChangeMultiplier;
            _projectileForce = Mathf.Clamp(_projectileForce, _currentWeaponData.GlobalWeaponData.MinProjectileForce, _currentWeaponData.GlobalWeaponData.MaxProjectileForce);
            
            NotifyProjectileForceChange();
        }

        void NotifyProjectileForceChange() {
            if (Mathf.Abs(_projectileForce - _lastProjectileForce) < 0.01f) return;
            
            _lastProjectileForce = _projectileForce;
            OnProjectileForceChanged?.Invoke(_projectileForce);
            _spawnedWeapon?.SetWeaponTension(_projectileForce);
        }

        public bool TryFire(out Projectile projectile) {
            projectile = null;
            var weapon = GetSpawnedWeapon();
            if (weapon == null) {
                Debug.LogError("Cannot fire: No weapon spawned.");
                return false;
            }

            // Get fitting tuple to find ammunition
            ProjectileData.ProjectileCategory? targetCategory = null;
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

            // Get shooter position from the player (owner of this WeaponStash)
            var shooterPosition = transform.root.position;
            projectile = weapon.FireWeapon(_projectileForce, shooterPosition);
            
            OnSuccessfulShot?.Invoke(projectile);
            OnAmmoChanged?.Invoke(targetCategory.Value, entry.weaponData, entry.ammo);
            return true;
        }

        public Weapon GetSpawnedWeapon() => _spawnedWeapon;

        public void DespawnSelectedWeapon() {
            if(_spawnedWeapon != null) {
                _spawnedWeapon.Dispose();
            }
        }

        public void ResetShootingPower() {
            _projectileForce = StartProjectileForce;
            _lastProjectileForce = _projectileForce;
            
            OnProjectileForceChanged?.Invoke(_projectileForce);
        }

        public void PredictTrajectory() {
            if (_spawnedWeapon == null) return;
            
            _spawnedWeapon.PredictTrajectory(_projectileForce);
        }
    }
}
