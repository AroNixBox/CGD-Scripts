using Gameplay.Runtime.Player.Trajectory;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [RequireComponent(typeof(PlayerWeaponStash))]
    public class PlayerWeaponController : MonoBehaviour {
        [SerializeField] int minProjectileForce = 0;
        [SerializeField] int maxProjectileForce = 100;

        [Tooltip("How fast does the projectile force with player inputs")] 
        [SerializeField] int projectileForceChangeMultiplier = 1000;
        float _projectileForce;
        PlayerWeaponStash _weaponStash;

        void Awake() {
            _weaponStash = GetComponent<PlayerWeaponStash>();
        }
        
        public void PredictTrajectory() {
            // TODO: Kill Singleton
            if (TrajectoryPredictor.Instance == null) {
                Debug.LogError("No TrajectoryPredictor in Scene");
                return;
            }

            var currentWeaponData = _weaponStash.GetCurrentWeaponData();
            var currentProjectileData = currentWeaponData.ProjectileData;
            var spawnedWeaponData = _weaponStash.GetSpawnedWeapon();
            TrajectoryPredictor.Instance.PredictTrajectory(
                spawnedWeaponData.GetWeaponProperties(),
                _projectileForce, 
                currentProjectileData.ProjectilePrefab.mass,
                currentProjectileData.ProjectilePrefab.linearDamping
            );
        }
        
        public void FireWeapon() {
            var currentWeaponData = _weaponStash.GetCurrentWeaponData();
            
            // Projectile
            var projectileData = currentWeaponData.ProjectileData;
            var projectilePrefab = projectileData.ProjectilePrefab;
            
            // Spawned Weapon
            var spawnedWeapon = _weaponStash.GetSpawnedWeapon();
            var currentWeaponProperties = spawnedWeapon.GetWeaponProperties();
            
            var projectile = Instantiate(projectilePrefab, currentWeaponProperties.MuzzlePosition, Quaternion.identity);
            projectile.AddForce(currentWeaponProperties.ShootDirection * _projectileForce, ForceMode.Impulse);
        }

        public void IncreaseProjectileForce() {
            _projectileForce += Time.deltaTime * projectileForceChangeMultiplier;
            _projectileForce = Mathf.Clamp(_projectileForce, minProjectileForce, maxProjectileForce);
        }

        public void DecreaseProjectileForce() {
            _projectileForce -= Time.deltaTime * projectileForceChangeMultiplier;
            _projectileForce = Mathf.Clamp(_projectileForce, minProjectileForce, maxProjectileForce);
        }

        public void ResetProjectileForce() => _projectileForce = minProjectileForce;
    }
}