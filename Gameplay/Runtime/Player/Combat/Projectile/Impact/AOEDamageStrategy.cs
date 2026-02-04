using System;
using System.Collections.Generic;
using Gameplay.Runtime.Interfaces;
using Gameplay.Runtime.Interfaces.Effects;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Gameplay.Runtime.Player.Combat {
    [Serializable]
    public class AOEDamageStrategy : IImpactStrategy {
        [SerializeField] uint aoeRadius = 1;

        [Header("Damage")] [SerializeField] float maximumDamage;
        [SerializeField] AnimationCurve damageDropoffCurve;

        [Tooltip("Enable to scale damage based on distance from shooter to impact point.")] [SerializeField]
        bool useRangeMultiplier;

        [ShowIf("useRangeMultiplier")]
        [Tooltip("Maximum distance for the range damage ramp-up effect.")]
        [SerializeField]
        float maxRangeRampUp = 50f;

        [ShowIf("useRangeMultiplier")]
        [Tooltip(
            "Curve to evaluate damage multiplier based on distance from shooter to impact. X-axis is normalized distance (0-1), Y-axis is damage multiplier.")]
        [SerializeField]
        AnimationCurve rangeRampUpCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);

        [Header("Physical Impact")]
        [Tooltip("Maximum explosion force that will push (>0) or pull (<0) the player in direction of the impact.")]
        [SerializeField]
        float maximumExplosionForce;

        [Tooltip(
            "Explosion strength in vertical direction. On top of normal force and also invertable for downward force.")]
        [SerializeField]
        float maximumExplosionUpwardModifier;

        [Tooltip(
            "Knockback intensity based on distance from impact center. X-axis is normalized distance (0-1), Y-axis is knockback multiplier.")]
        [SerializeField]
        AnimationCurve knockbackDropoffCurve = AnimationCurve.Linear(0, 1f, 1, 0f);

        [Tooltip("Enable to scale knockback based on distance from shooter to impact point.")] [SerializeField]
        bool useRangeKnockbackMultiplier;

        [ShowIf("useRangeKnockbackMultiplier")]
        [Tooltip("Maximum distance for the range knockback ramp-up effect.")]
        [SerializeField]
        float maxRangeKnockbackRampUp = 50f;

        [ShowIf("useRangeKnockbackMultiplier")]
        [Tooltip(
            "Curve to evaluate knockback multiplier based on distance from shooter to impact. X-axis is normalized distance (0-1), Y-axis is knockback multiplier.")]
        [SerializeField]
        AnimationCurve rangeKnockbackRampUpCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);
        
        [Header("Effects")]
        [Tooltip("Camera shake intensity (independent of damage).")]
        [SerializeField]
        float maxImpulseForce = 3f;
        
        [Tooltip("Number of shake waves to generate.")]
        [SerializeField]
        int waveCount = 5;

        [Tooltip("Time between each wave in seconds.")]
        [SerializeField]
        float waveInterval = 0.08f;

        [Header("Visual Effects")]
        [Tooltip("VFX Prefab that will be spawned at the impact position (ground debris, rocks, etc).")]
        [SerializeField]
        GameObject impactVfxPrefab;


        public float MaximumDamage => maximumDamage;
        public float MaximumExplosionForce => maximumExplosionForce;

        public ImpactResult OnImpact(ImpactData impactData) {
            // Calculate range multiplier for damage if enabled
            float rangeMultiplier = 1f;
            if (useRangeMultiplier && maxRangeRampUp > 0) {
                float distanceFromShooter = Vector3.Distance(impactData.ShooterPosition, impactData.Position);
                float normalizedDistance = Mathf.Clamp01(distanceFromShooter / maxRangeRampUp);
                rangeMultiplier = rangeRampUpCurve.Evaluate(normalizedDistance);
            }

            // Calculate range multiplier for knockback if enabled
            float rangeKnockbackMultiplier = 1f;
            if (useRangeKnockbackMultiplier && maxRangeKnockbackRampUp > 0) {
                float distanceFromShooter = Vector3.Distance(impactData.ShooterPosition, impactData.Position);
                float normalizedDistance = Mathf.Clamp01(distanceFromShooter / maxRangeKnockbackRampUp);
                rangeKnockbackMultiplier = rangeKnockbackRampUpCurve.Evaluate(normalizedDistance);
            }

            return OnImpactInternal(impactData.Position, rangeMultiplier, rangeKnockbackMultiplier);
        }

        public ImpactResult OnImpact(Vector3 impactPosition) {
            return OnImpactInternal(impactPosition, 1f, 1f);
        }

        ImpactResult OnImpactInternal(Vector3 impactPosition, float rangeMultiplier, float rangeKnockbackMultiplier) {
            // Spawn VFX at impact position
            SpawnImpactVfx(impactPosition);
            
            // Generate camera impulse at every impact
            GenerateCameraImpulse(impactPosition);
            
            var result = new ImpactResult {
                HitObjectOrigins = new List<Vector3>(),
                HitEntities = new List<Transform>()
            };
            var overlappedObjects = Physics.OverlapSphere(impactPosition, aoeRadius);

            bool hitAnyDamageable = false;

            foreach (var overlappedObject in overlappedObjects) {
                if (!overlappedObject.TryGetComponent(out IDamageable damageable))
                    continue;

                hitAnyDamageable = true;

                // TODO: This only uses the Center-Point of the overlaped object and should instead use the collisionpoint
                var distanceObjectFromCenter = Vector3.Distance(impactPosition, overlappedObject.transform.position);
                // Bring in relation 0-1 based ont he max radius
                // Clamp is needed because objects origin can be further away than aoeRadius due to using Origin instead of collision point
                var distanceScore = Mathf.Clamp(distanceObjectFromCenter / aoeRadius, 0, 1);
                var damageIntensity = damageDropoffCurve.Evaluate(distanceScore) * rangeMultiplier;

                var dmg = ApplyDamage(damageable, damageIntensity);
                
                var knockbackIntensity = knockbackDropoffCurve.Evaluate(distanceScore) * rangeKnockbackMultiplier;
                result.TotalKnockbackApplied = ApplyPhysics(damageable, knockbackIntensity, impactPosition);
                
                // Player specific logic
                if(!overlappedObject.TryGetComponent(out PlayerController _))
                    continue;
                
                // only log damage dealt to players
                result.TotalDamageDealt = dmg;
                
                // Only store players as targets
                // If the target is still alive, we can use it as tracking point
                if (damageable.GetHealth() > 0) {
                    result.HitEntities.Add(overlappedObject.transform);
                }
                else {
                    // Else store it in static Positions
                    result.HitObjectOrigins.Add(overlappedObject.transform.position);
                    var allColliders = overlappedObject.transform.GetComponentsInChildren<Collider>();
                    if (allColliders.Length > 0) {
                        var bounds = allColliders[0].bounds;
                        foreach (var col in allColliders) bounds.Encapsulate(col.bounds);

                        var topPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                        result.HitObjectOrigins.Add(topPoint);
                    }
                }
            }

            // If no damageables were hit, add an elevated point to prevent extreme camera zoom
            if (!hitAnyDamageable) {
                var elevatedPoint = impactPosition + Vector3.up * 3f;
                result.HitObjectOrigins.Add(elevatedPoint);
            }

            return result;
        }

        // TODO: Damage shouldnt be applied instant but go over POI! Player should be first to apply dmg or can be instant
        float ApplyDamage(IDamageable target, float intensity) {
            if (target == null) return 0f;

            var damage = maximumDamage * intensity;
            target.TakeDamage(damage);
            return damage;
        }

        float ApplyPhysics(IDamageable target, float intensity, Vector3 projectileImpactPosition) {
            if (target is not MonoBehaviour targetMonoBehaviour) return 0f;

            // CalculateForce
            var explosionForce = Mathf.Abs(maximumExplosionForce * intensity);
            var explosionUpwardsModifier = Mathf.Abs(maximumExplosionUpwardModifier * intensity);
            var impactDirection = (targetMonoBehaviour.transform.position - projectileImpactPosition).normalized *
                                  (maximumExplosionForce > 0 ? 1 : -1);
            var totalForce = impactDirection * explosionForce + Vector3.up * explosionUpwardsModifier;
            var totalForceMagnitude = totalForce.magnitude;
            Debug.Log(totalForceMagnitude);

            // Player Controller uses own Physics System
            if (targetMonoBehaviour.TryGetComponent(out PlayerController playerController)) {
                playerController.ApplyExternalForce(totalForce);
            } // Normal Rigidbodies
            else if (targetMonoBehaviour.TryGetComponent(out Rigidbody targetRigidbody)) {
                targetRigidbody.AddForce(impactDirection * explosionForce, ForceMode.Impulse);
                targetRigidbody.AddForce(
                    (maximumExplosionUpwardModifier > 0 ? Vector3.up : Vector3.down) * explosionUpwardsModifier,
                    ForceMode.Impulse);
            }

            return totalForceMagnitude;
        }

        void GenerateCameraImpulse(Vector3 impactPosition) {
            if (maxImpulseForce <= 0) return;

            // Calculate duration per wave based on impulse strength
            float waveDuration = Mathf.Lerp(0.05f, 0.15f, maxImpulseForce / 10f);

            // Start coroutine-like behavior using a MonoBehaviour helper
            var impulseHelper = new GameObject("ImpulseWaveHelper");
            var helper = impulseHelper.AddComponent<ImpulseWaveGenerator>();
            helper.StartWaves(impactPosition, maxImpulseForce, waveCount, waveInterval, waveDuration);
        }
        
        void SpawnImpactVfx(Vector3 impactPosition) {
            if (impactVfxPrefab == null) return;
            
            Object.Instantiate(impactVfxPrefab, impactPosition, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// Helper MonoBehaviour to generate multiple impulse waves over time
    /// </summary>
    public class ImpulseWaveGenerator : MonoBehaviour {
        public void StartWaves(Vector3 position, float baseStrength, int waves, float interval, float waveDuration) {
            StartCoroutine(GenerateWaves(position, baseStrength, waves, interval, waveDuration));
        }
        
        System.Collections.IEnumerator GenerateWaves(Vector3 position, float baseStrength, int waves, float interval, float waveDuration) {
            for (int i = 0; i < waves; i++) {
                // Each wave gets progressively weaker (1.0 -> 0.2)
                float waveMultiplier = Mathf.Lerp(1f, 0.2f, (float)i / (waves - 1));
                float waveStrength = baseStrength * waveMultiplier;
                
                // Create impulse source for this wave
                var impulseGo = new GameObject($"ImpulseWave_{i}");
                impulseGo.transform.position = position;
                var impulseSource = impulseGo.AddComponent<CinemachineImpulseSource>();
                
                // Configure the impulse
                impulseSource.ImpulseDefinition.ImpulseChannel = -1;
                impulseSource.ImpulseDefinition.ImpulseDuration = waveDuration;
                impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
                impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
                impulseSource.ImpulseDefinition.DissipationRate = 0.5f;
                
                // Generate this wave
                impulseSource.GenerateImpulseWithForce(waveStrength);
                
                // Destroy this wave's source after it completes
                Destroy(impulseGo, waveDuration + 0.2f);
                
                // Wait before next wave (except for last one)
                if (i < waves - 1) {
                    yield return new WaitForSeconds(interval);
                }
            }
            
            // Destroy the helper
            Destroy(gameObject, 0.1f);
        }
    }
}