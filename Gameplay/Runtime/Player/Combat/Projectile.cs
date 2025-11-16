using System;
using Common.Runtime;
using Common.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour {
        [Header("Settings")]
        [Tooltip("Bullet never Triggers Action (Bugged out), falls back on lifetime for dispose and self-destruction")]
        [SerializeField] float fallbackLifetime = 15;
        
        Rigidbody _rb;
        bool _hasImpacted;
        
        CountdownTimer _lifetimeTimer;
        Action _onProjectileExpired; // All Actions completed, ready to be disposed

        ProjectileImpactData _impactData;
        CountdownTimer _projectileActionTimer;

        void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        // "Constructor"
        public void Init(float mass, float drag, float force, Vector3 direction, Action onCompleteAction, ProjectileImpactData impactData) {
            if(impactData == null)
                throw new NullReferenceException("Impact Data null");
            
            _impactData = impactData;

            if (IsCountdown()) {
                // TODO: If pool reuse old Timer and reassign initialTime
                _projectileActionTimer = new CountdownTimer(impactData.GetProjectileActionCountdown());
                _projectileActionTimer.Start();
                _projectileActionTimer.OnTimerStop += ApplyDamage;
            }
            
            if (onCompleteAction == null) {
                Debug.LogWarning("No Projectile-Complete Action Implemented");
            }
            else {
                _onProjectileExpired = onCompleteAction;
            }
            
            _lifetimeTimer ??= new CountdownTimer(fallbackLifetime);
            _lifetimeTimer.Start();
            _lifetimeTimer.OnTimerStop += Dispose;

            _rb.mass = mass;
            _rb.linearDamping = drag;
            _rb.AddForce(force * direction, ForceMode.Impulse);
        }

        void OnCollisionEnter(Collision _) {
            // Currently only supports single Collision
            if (!IsSingleCollision()) return;
            if(_hasImpacted) return;
            _hasImpacted = true;
            

            ApplyDamage();
            // TODO: Force
            // TODO: VFX & SFX w/callback for effect ended
        }
        
        void ApplyDamage() {
            var aoeRadius = _impactData.GetAOERadius();
            var overlappedObjects = Physics.OverlapSphere(transform.position, aoeRadius);
            
            foreach (var overlappedObject in overlappedObjects) {
                if (!overlappedObject.TryGetComponent(out IDamageable damageable))
                    continue;
                
                // TODO: This only uses the Center-Point of the overlaped object and should instead use the collisionpoint
                var distanceObjectFromCenter = Vector3.Distance(transform.position, overlappedObject.transform.position);
                // Bring in relation 0-1 based ont he max radius
                // Clamp is needed because objects origin can be further away than aoeRadius due to using Origin instead of collision point
                var distanceScore = Mathf.Clamp(distanceObjectFromCenter / aoeRadius, 0, 1); 
                var damageScore = _impactData.GetDropOffCurve().Evaluate(distanceScore); 
                var damage = _impactData.GetMaximumDamage() * damageScore;

                if (damageable is MonoBehaviour damageableMB) {
                    Debug.Log($"Dealing {damage} Damage to {damageableMB.gameObject.name}");
                }

                damageable.TakeDamage(damage);
            }
            
            Dispose();
            Destroy(gameObject); // TODO Pool
        }

        void Update() {
            _lifetimeTimer?.Tick(Time.deltaTime);
            _projectileActionTimer?.Tick(Time.deltaTime);
        }
        
        void Dispose() {
            if (_lifetimeTimer != null) {
                _lifetimeTimer.OnTimerStop -= Dispose;
                _lifetimeTimer.Reset();
            }
            else {
                Debug.LogError("No lifetime timer available, did you Initialize this object via Init()?");
            }

            // Null when using a different
            if (_projectileActionTimer != null) {
                _projectileActionTimer.OnTimerStop -= ApplyDamage;
                _projectileActionTimer.Reset();
            } else if (IsCountdown()) {
                Debug.LogError("Selected was Projectile-Type Countdown but Projectile Action Trigger is null, did you change the Projectile-Action-Trigger in Runtime?");
            }

            _hasImpacted = false;
            _onProjectileExpired?.Invoke();
            Destroy(gameObject);
        }
        
        // Helper
        bool IsSingleCollision() => _impactData.GetProjectileActionTrigger() ==
                                    ProjectileImpactData.EProjectileActionTrigger.FirstCollision;
        
        bool IsCountdown() => _impactData.GetProjectileActionTrigger() ==
                                    ProjectileImpactData.EProjectileActionTrigger.Countdown;
    }
}
