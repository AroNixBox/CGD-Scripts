using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.Runtime.Player.Trajectory {
    public class ProjectileThrow : MonoBehaviour {
        [SerializeField] TrajectoryPredictor trajectoryPredictor;
        [SerializeField] Rigidbody prefabRb;
        [SerializeField] float throwForce;
        [SerializeField] Transform startTransform;
        const float StartTransformForwardOffset = 1f;

        void Update() {
            var projectileProperties = new ProjectileProperties(
                initialSpeed: throwForce,
                mass: prefabRb.mass,
                drag: prefabRb.linearDamping
            );
            var weaponProperties = new WeaponProperties(
                startTransform.forward,
                GetStartPosition()
            );
            trajectoryPredictor.PredictTrajectory(weaponProperties, projectileProperties);

            if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
                var thrownObject = Instantiate(prefabRb, GetStartPosition(), Quaternion.identity);
                thrownObject.AddForce(startTransform.forward * throwForce, ForceMode.Impulse);
            }
        }

        Vector3 GetStartPosition() {
            return startTransform.position + startTransform.forward * StartTransformForwardOffset;
        }
    }
}