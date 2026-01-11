using System;
using Common.Runtime;
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

        ProjectileImpactData _impactData;
        
        /// <summary>
        /// Event fired when the projectile impacts. Parameters: impactPosition, wasActiveImpact (true if not fallback lifetime), impactResult
        /// </summary>
        public event Action<Vector3, ImpactResult> OnImpact;
        CountdownTimer _projectileActionTimer;

        void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        // "Constructor"
        public void Init(float mass, float drag, float force, Vector3 direction, ProjectileImpactData impactData) {
            if(impactData == null)
                throw new NullReferenceException("Impact Data null");
            
            _impactData = impactData;

            if (_impactData.GetProjectileActionTrigger() == 
                ProjectileImpactData.EProjectileActionTrigger.Countdown) {
                _projectileActionTimer = new CountdownTimer(impactData.GetProjectileActionCountdown());
                _projectileActionTimer.Start();
                _projectileActionTimer.OnTimerStop += OnProjectileTimerActionComplete;
            }
            
            _lifetimeTimer ??= new CountdownTimer(fallbackLifetime);
            _lifetimeTimer.Start();
            _lifetimeTimer.OnTimerStop += OnFallbackExpired;

            _rb.mass = mass;
            _rb.linearDamping = drag;
            _rb.AddForce(force * direction, ForceMode.Impulse);
        }
        
        void OnCollisionEnter(Collision collision) {
            if (_impactData.GetProjectileActionTrigger() !=
                ProjectileImpactData.EProjectileActionTrigger.FirstCollision) return;
            
            CompleteImpact(collision);
        }

        // Called when Projectile-Action that is triggered by timer, timer = 0
        void OnProjectileTimerActionComplete() {
            CompleteImpact(null);
        }

        void Update() {
            _lifetimeTimer?.Tick(Time.deltaTime);
            _projectileActionTimer?.Tick(Time.deltaTime);
        }

        void OnFallbackExpired() {
            CompleteImpact(null);
        }

        void CompleteImpact(Collision collision) {
            if (_hasImpacted) return;
            _hasImpacted = true;
            
            // Impact Strategy
            var impactStrategy = _impactData.GetImpactStrategy();
            
            // Use collision data if available, otherwise fallback to transform position
            var impactData = collision is { contactCount: > 0 }
                ? ImpactData.FromCollision(collision)
                : ImpactData.FromPosition(transform.position);
            
            var impactResult = impactStrategy.OnImpact(impactData);
            
            ApplyEffects();
            
            // Fire impact event before cleanup
            OnImpact?.Invoke(transform.position, impactResult);
            
            if (_lifetimeTimer != null) {
                _lifetimeTimer.OnTimerStop -= OnFallbackExpired;
                _lifetimeTimer.Reset();
            }
            else {
                Debug.LogError("No lifetime timer available, did you Initialize this object via Init()?");
            }

            if (_projectileActionTimer != null) {
                _projectileActionTimer.Reset();
            } else if (_impactData.GetProjectileActionTrigger() == 
                       ProjectileImpactData.EProjectileActionTrigger.Countdown) {
                Debug.LogError("Selected was Projectile-Type Countdown but Projectile Action Trigger is null, did you change the Projectile-Action-Trigger in Runtime?");
            }

            Destroy(gameObject);
        }

        void ApplyEffects() { 
            // Sfx
            var impactSfx = _impactData.GetImpactSfx();
            PlayImpactSound(impactSfx);

            // Vfx
            var impactVfx = _impactData.GetImpactVfx();
            CreateImpactEffect(impactVfx);
        }
        
        // TODO: Audio Manager
        void PlayImpactSound(AudioClip clip) {
            if (clip == null)
                return;
            
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        
        // TODO: Effect-Manger & Pool?
        void CreateImpactEffect(GameObject effectPrefab) {
            if (effectPrefab == null)
                return;
            
            if (effectPrefab != null)
                Instantiate(effectPrefab, transform.position, Quaternion.identity);
        }
    }
}


