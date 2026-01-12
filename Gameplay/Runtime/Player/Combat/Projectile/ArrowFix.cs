using System;
using UnityEngine;

namespace Gameplay.Runtime {
    public class ArrowFix : MonoBehaviour {
        private Rigidbody _rb;

        private void Start() {
            _rb = GetComponent<Rigidbody>();
        }
        
        private void Update() {
            transform.forward = _rb.linearVelocity.normalized;
        }
    }
}