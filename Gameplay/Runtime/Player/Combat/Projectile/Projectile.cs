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
        Action<bool> _onProjectileExpired; // bool = was the impact fallbackLifeTime = false or active through instant col/set countdown

        ProjectileImpactData _impactData;
        CountdownTimer _projectileActionTimer;

        void Awake() {
            _rb = GetComponent<Rigidbody>();
        }

        // "Constructor"
        public void Init(float mass, float drag, float force, Vector3 direction, Action<bool> onCompleteAction, ProjectileImpactData impactData) {
            if(impactData == null)
                throw new NullReferenceException("Impact Data null");
            
            _impactData = impactData;

            if (IsCountdown()) {
                // TODO: If pool reuse old Timer and reassign initialTime
                _projectileActionTimer = new CountdownTimer(impactData.GetProjectileActionCountdown());
                _projectileActionTimer.Start();
                _projectileActionTimer.OnTimerStop += OnProjectileTimerActionComplete;
            }
            
            if (onCompleteAction == null) {
                Debug.LogWarning("No Projectile-Complete Action Implemented");
            }
            else {
                _onProjectileExpired = onCompleteAction;
            }
            
            _lifetimeTimer ??= new CountdownTimer(fallbackLifetime);
            _lifetimeTimer.Start();
            _lifetimeTimer.OnTimerStop += OnFallbackExpired;

            _rb.mass = mass;
            _rb.linearDamping = drag;
            _rb.AddForce(force * direction, ForceMode.Impulse);
        }

        void OnCollisionEnter(Collision _) {
            // Currently only supports single Collision
            if (!IsSingleCollision()) return;
            if(_hasImpacted) return;
            _hasImpacted = true;
            
            CompleteImpact(wasActiveImpact: true);
        }

        // Called when Projectile-Action that is triggered by timer, timer = 0
        void OnProjectileTimerActionComplete() {
            CompleteImpact(wasActiveImpact: true);
        }

        void Update() {
            _lifetimeTimer?.Tick(Time.deltaTime);
            _projectileActionTimer?.Tick(Time.deltaTime);
        }

        void OnFallbackExpired() => CompleteImpact(wasActiveImpact: false);

        void CompleteImpact(bool wasActiveImpact) {
            // Impact Strategy
            var impactStrategy = _impactData.GetImpactStrategy();
            impactStrategy.OnImpact(transform.position);
            
            ApplyEffects();
            
            if (_lifetimeTimer != null) {
                _lifetimeTimer.OnTimerStop -= OnFallbackExpired;
                _lifetimeTimer.Reset();
            }
            else {
                Debug.LogError("No lifetime timer available, did you Initialize this object via Init()?");
            }

            if (_projectileActionTimer != null) {
                _projectileActionTimer.Reset();
            } else if (IsCountdown()) {
                Debug.LogError("Selected was Projectile-Type Countdown but Projectile Action Trigger is null, did you change the Projectile-Action-Trigger in Runtime?");
            }

            _hasImpacted = false;
            _onProjectileExpired?.Invoke(wasActiveImpact);
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

        // Helper
        bool IsSingleCollision() => _impactData.GetProjectileActionTrigger() ==
                                    ProjectileImpactData.EProjectileActionTrigger.FirstCollision;
        
        bool IsCountdown() => _impactData.GetProjectileActionTrigger() ==
                                    ProjectileImpactData.EProjectileActionTrigger.Countdown;
    }
}


