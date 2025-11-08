using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour {
        Rigidbody _rb;

        void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        public void Init(Vector3 position, Vector3 velocity) {
            _rb.position = position;
            _rb.AddForce(velocity, ForceMode.Impulse);
        }

        public void ResetRigidbody() {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
}
