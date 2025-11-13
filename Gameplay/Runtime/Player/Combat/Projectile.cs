using System;
using Common.Runtime;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("Bullet never impacts and falls back on lifetime for dispose logic")]
        [SerializeField] float fallbackLifetime = 15;
        [Tooltip("Bullet Effect triggers")]
        [SerializeField] float velocityThreshold = 0.1f;

        
        Rigidbody _rb;
        bool _hasImpacted;
        
        CountdownTimer _lifetimeTimer;
        Action _onProjectileExpired; // All Actions completed, ready to be disposed

        void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        public void Init(float mass, float drag, float force, Vector3 direction, Action onCompleteAction) {
            _onProjectileExpired = onCompleteAction;
            _onProjectileExpired += Dispose;
            
            _lifetimeTimer ??= new CountdownTimer(fallbackLifetime);
            _lifetimeTimer.Start();
            // TODO:
            _lifetimeTimer.OnTimerStop += _onProjectileExpired;

            _rb.mass = mass;
            _rb.linearDamping = drag;
            _rb.AddForce(force * direction, ForceMode.Impulse);
        }

        void OnCollisionEnter(Collision other) {
            _hasImpacted = true;
            // TODO: Damage
            // TODO: Force
            // TODO: VFX & SFX w/callback for effect ended
        }

        void Update() => _lifetimeTimer?.Tick(Time.deltaTime);

        void FixedUpdate() {
            if (!_hasImpacted) return;
            // TODO: Could check here if we are grounded via contact points
            if (_rb.linearVelocity.magnitude > velocityThreshold) return;
            
            _onProjectileExpired.Invoke();
            Dispose();
        }

        
        void Dispose() {
            _lifetimeTimer.OnTimerStop -= _onProjectileExpired;
            _lifetimeTimer.Reset();

            _hasImpacted = false;
            Destroy(gameObject);
        }
    }
}
