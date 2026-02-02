using System;
using System.Collections.Generic;
using Core.Runtime.Service;
using Gameplay.Runtime.Player.Trajectory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    public class Weapon : MonoBehaviour {
        [Tooltip("Bullet Spawn Point")]
        [SerializeField, Required] Transform muzzlePoint;
        [SerializeField] Animator animator;
        [SerializeField] List<Renderer> meshRenderers;

        private MaterialPropertyBlock mpb;

        static readonly int Force = Animator.StringToHash("Force");
        
        WeaponData _weaponData;
        TrajectoryPredictor _trajectoryPredictor;

        public void Init(WeaponData weaponData) {
            _weaponData = weaponData;
            mpb = new MaterialPropertyBlock();
            if (!ServiceLocator.TryGet(out _trajectoryPredictor))
                throw new NullReferenceException("Trajectory Projector not available via Service Locator");
        }
        
        public void SetWeaponTension(float currentPercent) {
            if (animator != null)
                animator?.SetFloat(Force, currentPercent / 100);

            if (meshRenderers.Count > 0) {
                mpb.Clear();
                mpb.SetFloat("_Activity", currentPercent / 100);

                foreach (var mat in meshRenderers) {
                    mat.SetPropertyBlock(mpb);
                }
            }
        }

        WeaponProperties GetWeaponProperties() {
            return new WeaponProperties(
                muzzlePoint.forward,
                muzzlePoint.position
            );
        }

        public void PredictTrajectory(float force) => 
            _trajectoryPredictor.PredictTrajectory(
                GetWeaponProperties(),
                force, 
                _weaponData.ProjectileData.Mass,
                _weaponData.ProjectileData.Drag
            );

        public Projectile FireWeapon(float force, Vector3 shooterPosition) {
            // Projectile
            var projectileData = _weaponData.ProjectileData;
            var projectilePrefab = projectileData.ProjectilePrefab;
            
            // Spawned Weapon
            var projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            projectile.Init(
                projectileData.Mass, 
                projectileData.Drag, 
                force, 
                muzzlePoint.forward, 
                _weaponData.ProjectileData.ImpactData,
                shooterPosition);

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
            Destroy(gameObject); // TODO: Pool?
        }
    }
}