using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime.Player.Trajectory {
    public class ProjectileShooter : MonoBehaviour {
        [SerializeField] Projectile projectile;
        [SerializeField] Projection projection;
        [SerializeField] float projectileForce;

        [SerializeField] Transform activePlayerCamera;
        const float SpawnOffset = 1f;

        void Update() {
            projection.SimulateTrajectory(projectile, activePlayerCamera.position + activePlayerCamera.forward * SpawnOffset, activePlayerCamera.forward * projectileForce);

            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) {
                var projectileClone = Instantiate(projectile.gameObject,
                    activePlayerCamera.position + activePlayerCamera.forward * SpawnOffset, Quaternion.identity);
                projectileClone.GetComponent<Projectile>().Init(activePlayerCamera.forward * projectileForce);
            }
        }
    }
}
