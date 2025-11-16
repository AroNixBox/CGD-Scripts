using System;
using Gameplay.Runtime.Player.Trajectory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class Weapon : MonoBehaviour {
        [Tooltip("Bullet Spawn Point")]
        [SerializeField, Required] public Transform muzzlePoint;
        
        // TODO: Data Object?
        [SerializeField] int minProjectileForce = 0;
        [SerializeField] int maxProjectileForce = 100;
        [Tooltip("How fast does the projectile force with player inputs")] 
        [SerializeField] int projectileForceChangeMultiplier = 1000;
        float _projectileForce;
        
        WeaponData _weaponData;

        public void Init(WeaponData weaponData) {
            _weaponData = weaponData;
        }

        public WeaponProperties GetWeaponProperties() {
            return new WeaponProperties(
                muzzlePoint.forward,
                muzzlePoint.position
            );
        }
        
        public void PredictTrajectory() {
            // TODO: Kill Singleton
            if (TrajectoryPredictor.Instance == null) {
                Debug.LogError("No TrajectoryPredictor in Scene");
                return;
            }

            TrajectoryPredictor.Instance.PredictTrajectory(
                GetWeaponProperties(),
                _projectileForce, 
                _weaponData.ProjectileData.mass,
                _weaponData.ProjectileData.drag
            );
        }
        
        public void IncreaseProjectileForce() {
            _projectileForce += Time.deltaTime * projectileForceChangeMultiplier;
            _projectileForce = Mathf.Clamp(_projectileForce, minProjectileForce, maxProjectileForce);
        }

        public void DecreaseProjectileForce() {
            _projectileForce -= Time.deltaTime * projectileForceChangeMultiplier;
            _projectileForce = Mathf.Clamp(_projectileForce, minProjectileForce, maxProjectileForce);
        }
        
        public Projectile FireWeapon(Action onProjectileExpired) {
            // Projectile
            var projectileData = _weaponData.ProjectileData;
            var projectilePrefab = projectileData.projectilePrefab;
            
            // Spawned Weapon
            var projectile = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.identity);
            projectile.Init(
                projectileData.mass, 
                projectileData.drag, 
                _projectileForce, 
                muzzlePoint.forward, 
                onProjectileExpired, 
                _weaponData.ProjectileData.impactData);

            if (_weaponData.ProjectileData.FiredSound != null)
                PlayFireSound(_weaponData.ProjectileData.FiredSound);
            
            if (_weaponData.ProjectileData.MuzzleEffect != null)
                CreateFireEffect(_weaponData.ProjectileData.MuzzleEffect);
            
            return projectile;
        }
        
        // TODO: Audio Manager
        void PlayFireSound(AudioClip clip) {
            if (clip == null)
                return;

            if (clip == null) return;
            
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        
        // TODO: Effect-Manger & Pool?
        void CreateFireEffect(GameObject effectPrefab) {
            if (effectPrefab == null)
                return;

            if (effectPrefab == null) return;
            
            var spawnedEffect = Instantiate(effectPrefab, muzzlePoint.position, Quaternion.identity);
            spawnedEffect.transform.forward = muzzlePoint.forward;
        }
        
        // TODO:
        // Needs to be able to be rotated with the Camera
        // Or should this rather be done with the entire arm via IK

        public void Dispose() {
            _projectileForce = 0;
            Destroy(gameObject); // TODO: Pool?
        }
    }
}