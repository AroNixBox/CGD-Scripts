using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using Gameplay.Runtime.Player.Combat;
using Sirenix.Utilities;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    public class ProjectileWatchState : IState {
        readonly PlayerController _controller;
        readonly PlayerCameraControls _cameraControls;
        readonly Func<Projectile> _getProjectile;
        
        Projectile _projectile;

        public bool IsComplete { get; private set; }

        public ProjectileWatchState(PlayerController controller, Func<Projectile> getProjectile) {
            _controller = controller;
            _cameraControls = controller.PlayerCameraControls;
            _getProjectile = getProjectile;
        }

        public void OnEnter() {
            _projectile = _getProjectile();
            if(_projectile == null)
                throw new NullReferenceException("ProjectileWatchState entered but no projectile found");
            
            IsComplete = false;
            
            _cameraControls.EnableBulletCamera(_projectile.transform);
            _projectile.OnImpact += (impactPosition, impactResult) => HandleProjectileImpact(impactPosition, impactResult).Forget();
        }

        /// <summary>
        /// The Projectile expired (either in the eir due to fallback or in the ground...)
        /// We could try to zoom out and capture all POI's
        /// </summary>
        async UniTaskVoid HandleProjectileImpact(Vector3 impactPosition, ImpactResult impactResult) {
            var projectileImpactPosDummy = new GameObject("Projectile Impact Position") {
               transform = {
                   position = impactPosition
               }
            };
           
            // A Damageable can be destroyed, and we still want to observer the point where the damageable was :)
            // Create list with impact position + all POIs
            var hitObjectOriginDummies = new List<Transform> { projectileImpactPosDummy.transform };
            foreach (var hitObjectOriginPos in impactResult.HitObjectOrigins) {
                var hitObjectDummyTransform = new GameObject("Hit Object Transform") {
                    transform = {
                        position = hitObjectOriginPos
                    }
                }; 
                hitObjectOriginDummies.Add(hitObjectDummyTransform.transform);
            }
           
            _cameraControls.EnableImpactCamera(hitObjectOriginDummies);
            _cameraControls.ResetBulletCamera();
            
            // Wait for post-impact delay
            await UniTask.WaitForSeconds(_controller.PostImpactDelay * 3);
            _cameraControls.ResetImpactCamera();
            CompleteState();
        }

        void CompleteState() {
            _controller.AuthorityEntity.GiveNextAuthority();
            IsComplete = true;
        }

        public void Tick(float deltaTime) {
            // Slowmo, Zoom, etc.
        }

        public void OnExit() { }

        public Color GizmoState() => Color.cyan;
    }
}

