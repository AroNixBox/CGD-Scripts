using UnityEngine;

namespace Gameplay.Runtime {
    public class BallFix : MonoBehaviour {
        [SerializeField] float spinStrength = 0.5f;

        private Rigidbody _rb;

        private void Start() {
            _rb = GetComponent<Rigidbody>();

            //_rb.AddTorque(Random.onUnitSphere * spinStrength, ForceMode.Impulse);

            _rb.AddTorque(transform.right * spinStrength, ForceMode.Impulse);
        }
    }
}