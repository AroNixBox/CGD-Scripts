using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime.Player.Trajectory {
    public class ProjectileShooter : MonoBehaviour {
        [SerializeField] Projectile projectile;
        [SerializeField] Projection projection;
        [SerializeField] float projectileForce;

        [SerializeField] Transform activePlayerCamera;
        const float SpawnOffset = 1f;

        void Awake() {
            projection.InitializePool(projectile);
        }

        void Update() {
            if(Mouse.current != null && Mouse.current.rightButton.isPressed) return;
            
            projection.SimulateTrajectory(activePlayerCamera.position + activePlayerCamera.forward * SpawnOffset, activePlayerCamera.forward * projectileForce);

            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) {
                var projectileClone = Instantiate(projectile);
                projectileClone.Init(activePlayerCamera.position + activePlayerCamera.forward * SpawnOffset, activePlayerCamera.forward * projectileForce);
            }
        }
    }
}
