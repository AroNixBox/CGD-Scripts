using Gameplay.Runtime.Player.Trajectory;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [RequireComponent(typeof(PlayerWeaponStash))]
    public class PlayerWeaponController : MonoBehaviour {
        PlayerWeaponStash _weaponStash;

        void Awake() {
            _weaponStash = GetComponent<PlayerWeaponStash>();
        }

        public void FireWeapon() {
            var currentWeaponData = _weaponStash.GetCurrentWeaponData();
            
            // Projectile
            var projectileData = currentWeaponData.ProjectileData;
            var currentProjectileProperties = projectileData.GetProjectileProperties();
            var projectilePrefab = projectileData.Projectile;
            
            // Spawned Weapon
            var spawnedWeapon = _weaponStash.GetSpawnedWeapon();
            var currentWeaponProperties = spawnedWeapon.GetWeaponProperties();
            
            var projectile = Instantiate(projectilePrefab, currentWeaponProperties.MuzzlePosition, Quaternion.identity);
            projectile.AddForce(currentWeaponProperties.ShootDirection * currentProjectileProperties.InitialSpeed, ForceMode.Impulse);
        }
    }
}