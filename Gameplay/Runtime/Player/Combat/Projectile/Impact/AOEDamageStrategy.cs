using System;
 using System.Collections.Generic;
 using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [Serializable]
    public class AOEDamageStrategy : IImpactStrategy {
        [SerializeField] uint aoeRadius = 1;
        [SerializeField] AnimationCurve damageDropoffCurve;
        [SerializeField] float maximumDamage;
        
        // TODO:
        [Header("Physical Impact")]
        [Tooltip("Maximum explosion force that will push (>0) or pull (<0) the player in direction of the impact.")]
        [SerializeField] float maximumExplosionForce;
        [Tooltip("Explosion strength in vertical direction. On top of normal force and also invertable for downward force.")]
        [SerializeField] float maximumExplosionUpwardModifier;

        public float MaximumDamage => maximumDamage;
        public float MaximumExplosionForce => maximumExplosionForce;
        
        // TODO: Implement
        // [field: SerializeField] public float DropoffBeginDistance { get; private set; }
        // [field: SerializeField] public float DropoffEndDistance { get; private set; }
        // [field: SerializeField] public float DistanceMultiplierValue { get; private set; }
        public ImpactResult OnImpact(Vector3 impactPosition) {
            var result = new ImpactResult {
                HitObjectOrigins = new List<(Transform, Vector3)>()
            };
            var overlappedObjects = Physics.OverlapSphere(impactPosition, aoeRadius);
            
            bool hitAnyDamageable = false;
            
            foreach (var overlappedObject in overlappedObjects) {
                // TODO
                if (!overlappedObject.TryGetComponent(out IDamageable damageable))
                    continue;

                hitAnyDamageable = true;

                // TODO: This only uses the Center-Point of the overlaped object and should instead use the collisionpoint
                var distanceObjectFromCenter = Vector3.Distance(impactPosition, overlappedObject.transform.position);
                // Bring in relation 0-1 based ont he max radius
                // Clamp is needed because objects origin can be further away than aoeRadius due to using Origin instead of collision point
                var distanceScore = Mathf.Clamp(distanceObjectFromCenter / aoeRadius, 0, 1); 
                var intensity = damageDropoffCurve.Evaluate(distanceScore);

                result.HitObjectOrigins.Add((overlappedObject.transform, overlappedObject.transform.position));

                var allColliders = overlappedObject.transform.GetComponentsInChildren<Collider>();
                if (allColliders.Length > 0) {
                    var bounds = allColliders[0].bounds;
                    foreach (var col in allColliders) bounds.Encapsulate(col.bounds);
                    
                    var topPoint = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
                    result.HitObjectOrigins.Add((overlappedObject.transform, topPoint));
                }
                
                result.TotalDamageDealt = ApplyDamage(damageable, intensity);
                result.TotalKnockbackApplied = ApplyPhysics(damageable, intensity, impactPosition);
            }
            
            // If no damageables were hit, add an elevated point to prevent extreme camera zoom
            if (!hitAnyDamageable) {
                var elevatedPoint = impactPosition + Vector3.up * 3f;
                result.HitObjectOrigins.Add((null, elevatedPoint));
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