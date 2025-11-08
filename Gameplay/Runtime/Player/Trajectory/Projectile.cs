using System;
using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour {
        Rigidbody _rb;

        void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        public void Init(Vector3 velocity) {
            _rb.AddForce(velocity, ForceMode.Impulse);
        }
    }
}
