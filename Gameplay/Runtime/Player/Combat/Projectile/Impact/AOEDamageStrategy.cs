using System;
using System.Collections.Generic;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [Serializable]
    public class AOEDamageStrategy : IImpactStrategy {
        [SerializeField] uint aoeRadius = 1;
        
        [Header("Damage")]
        [SerializeField] float maximumDamage;
        [SerializeField] AnimationCurve damageDropoffCurve;
        [Tooltip("Enable to scale damage based on distance from shooter to impact point.")]
        [SerializeField] bool useRangeMultiplier;
        [ShowIf("useRangeMultiplier")]
        [Tooltip("Maximum distance for the range damage ramp-up effect.")]
        [SerializeField] float maxRangeRampUp = 50f;
        [ShowIf("useRangeMultiplier")]
        [Tooltip("Curve to evaluate damage multiplier based on distance from shooter to impact. X-axis is normalized distance (0-1), Y-axis is damage multiplier.")]
        [SerializeField] AnimationCurve rangeRampUpCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);
        
        [Header("Physical Impact")]
        [Tooltip("Maximum explosion force that will push (>0) or pull (<0) the player in direction of the impact.")]
        [SerializeField] float maximumExplosionForce;
        [Tooltip("Explosion strength in vertical direction. On top of normal force and also invertable for downward force.")]
        [SerializeField] float maximumExplosionUpwardModifier;
        [Tooltip("Knockback intensity based on distance from impact center. X-axis is normalized distance (0-1), Y-axis is knockback multiplier.")]
        [SerializeField] AnimationCurve knockbackDropoffCurve = AnimationCurve.Linear(0, 1f, 1, 0f);
        [Tooltip("Enable to scale knockback based on distance from shooter to impact point.")]
        [SerializeField] bool useRangeKnockbackMultiplier;
        
        [ShowIf("useRangeKnockbackMultiplier")]
        [Tooltip("Maximum distance for the range knockback ramp-up effect.")]
        [SerializeField] float maxRangeKnockbackRampUp = 50f;
        
        [ShowIf("useRangeKnockbackMultiplier")]
        [Tooltip("Curve to evaluate knockback multiplier based on distance from shooter to impact. X-axis is normalized distance (0-1), Y-axis is knockback multiplier.")]
        [SerializeField] AnimationCurve rangeKnockbackRampUpCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);
        

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
            var result = new ImpactResult {
                HitObjectOrigins = new List<Vector3>()
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
                var knockbackIntensity = knockbackDropoffCurve.Evaluate(distanceScore) * rangeKnockbackMultiplier;
                Debug.Log($"Distance Score: {distanceScore}, Knockback Intensity: {knockbackIntensity}");

                result.HitObjectOrigins.Add(overlappedObject.transform.position);

                var allColliders = overlappedObject.transform.GetComponentsInChildren<Collider>();
                if (allColliders.Length > 0) {
                    var bounds = allColliders[0].bounds;
                    foreach (var col in allColliders) bounds.Encapsulate(col.bounds);
                    
                    var topPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                    result.HitObjectOrigins.Add(topPoint);
                }
                float dmg = ApplyDamage(damageable, damageIntensity);
                
                // log only if player
                if (damageable is EntityHealth health) 
                    if (health.TryGetComponent(out PlayerController ctrl)) 
                        result.TotalDamageDealt = dmg;
                
                result.TotalKnockbackApplied = ApplyPhysics(damageable, knockbackIntensity, impactPosition);
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
            if(target == null) return 0f;
            
            var damage = maximumDamage * intensity;
            target.TakeDamage(damage);
            return damage;
        }
        
        float ApplyPhysics(IDamageable target, float intensity, Vector3 projectileImpactPosition) {
            if(target is not MonoBehaviour targetMonoBehaviour) return 0f;
    
            // CalculateForce
            var explosionForce = Mathf.Abs(maximumExplosionForce * intensity);
            var explosionUpwardsModifier = Mathf.Abs(maximumExplosionUpwardModifier * intensity);
            var impactDirection = (targetMonoBehaviour.transform.position - projectileImpactPosition).normalized * (maximumExplosionForce > 0 ? 1 : -1);
            var totalForce = impactDirection * explosionForce + Vector3.up * explosionUpwardsModifier;
            var totalForceMagnitude = totalForce.magnitude;
            Debug.Log(totalForceMagnitude);
            
            // Player Controller uses own Physics System
            if (targetMonoBehaviour.TryGetComponent(out PlayerController playerController)) {
                playerController.ApplyExternalForce(totalForce);
            } // Normal Rigidbodies
            else if (targetMonoBehaviour.TryGetComponent(out Rigidbody targetRigidbody)) {
                targetRigidbody.AddForce(impactDirection * explosionForce, ForceMode.Impulse);
                targetRigidbody.AddForce((maximumExplosionUpwardModifier > 0 ? Vector3.up : Vector3.down) * explosionUpwardsModifier, ForceMode.Impulse);
            }

            return totalForceMagnitude;
        }
    }
}