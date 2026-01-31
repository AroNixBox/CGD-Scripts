using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using Gameplay.Runtime.Player.Combat;
using UnityEngine;
using UnityEngine.Pool;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    public class ProjectileWatchState : IState {
        // Shared static pool for all players to avoid N pools for N players and share resources
        static ObjectPool<Transform> _sharedCameraFocusPool;
        static Transform _sharedPoolRoot;

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

        static ObjectPool<Transform> GetFocusPool() {
            // Re-initialize if the root object was destroyed (e.g. scene load)
            if (_sharedPoolRoot != null) return _sharedCameraFocusPool;
            
            // New Pool
            _sharedPoolRoot = new GameObject("Shared Bullet Camera Focus Pool").transform;
            _sharedCameraFocusPool = new ObjectPool<Transform>(
                createFunc: () => {
                    var tr = new GameObject("FocusPoint").transform;
                    tr.SetParent(_sharedPoolRoot);
                    return tr;
                },
                actionOnGet: e => {
                    if (e != null) e.gameObject.SetActive(true);
                },
                actionOnRelease: e => {
                    if (e != null) e.gameObject.SetActive(false);
                },
                actionOnDestroy: e => {
                    if (e != null) UnityEngine.Object.Destroy(e.gameObject);
                },
                collectionCheck: true,
                defaultCapacity: 12,
                maxSize: 35
            );
            return _sharedCameraFocusPool;
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
            var projectileImpactPosDummy = GetObjectFromPool(impactPosition, "Projectile Impact Transform");
            
            // A Damageable can be destroyed, and we still want to observer the point where the damageable was :)
            // Create list with impact position + all POIs
            var staticFocusPoints = new List<Transform>();
            staticFocusPoints.Add(projectileImpactPosDummy);

            for (var i = 0; i < impactResult.HitObjectOrigins.Count; i++) {
                var cameraFocusPoint = GetObjectFromPool(impactResult.HitObjectOrigins[i], $"Hit Object Transform {i}");
                staticFocusPoints.Add(cameraFocusPoint);
            }

            var allFocusPoints = new List<Transform>();
            allFocusPoints.AddRange(staticFocusPoints);
            
            // TODO:
            if (impactResult.HitEntities != null) {
                foreach (var hitEntity in impactResult.HitEntities) {
                    allFocusPoints.Add(hitEntity);
                }
            }

            _cameraControls.SetBulletCameraTargets(allFocusPoints);
            
            // Wait for post-impact delay
            await UniTask.WaitForSeconds(_controller.PostImpactDelay * 3);
            _cameraControls.ResetBulletCamera();
            
            // Release objects back to the pool
            var pool = GetFocusPool();
            foreach (var point in staticFocusPoints) {
                if(point != null) pool.Release(point);
            }
            
            CompleteState();
        }

        Transform GetObjectFromPool(Vector3 newPosition, string objectName) {
            var poolTransform = GetFocusPool().Get();
            poolTransform.name = objectName;
            poolTransform.position = newPosition;
            return poolTransform;
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
