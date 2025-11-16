using System;
using Common.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [Serializable]
    public class AOEDamageStrategy : IImpactStrategy {
        [SerializeField] uint aoeRadius = 1;
        [SerializeField] AnimationCurve damageDropoffCurve;
        [SerializeField] float maximumDamage;
        
        // TODO:
        [Header("Physical Impact")]
        [SerializeField] float maximumExplosionForce;
        [SerializeField] float maximumExplosionUpwardModifier;
        
        // TODO: Implement
        // [field: SerializeField] public float DropoffBeginDistance { get; private set; }
        // [field: SerializeField] public float DropoffEndDistance { get; private set; }
        // [field: SerializeField] public float DistanceMultiplierValue { get; private set; }
        public void OnImpact(Vector3 impactPosition) {
            var overlappedObjects = Physics.OverlapSphere(impactPosition, aoeRadius);
                
            foreach (var overlappedObject in overlappedObjects) {
                if (!overlappedObject.TryGetComponent(out IDamageable damageable))
                    continue;

                // TODO: This only uses the Center-Point of the overlaped object and should instead use the collisionpoint
                var distanceObjectFromCenter = Vector3.Distance(impactPosition, overlappedObject.transform.position);
                // Bring in relation 0-1 based ont he max radius
                // Clamp is needed because objects origin can be further away than aoeRadius due to using Origin instead of collision point
                var distanceScore = Mathf.Clamp(distanceObjectFromCenter / aoeRadius, 0, 1); 
                var intensity = damageDropoffCurve.Evaluate(distanceScore); 
                
                ApplyDamage(damageable, intensity);
                ApplyPhysics(damageable, intensity, impactPosition);
            }
        }

        void ApplyDamage(IDamageable target, float intensity) {
            if(target == null) return; // Safety
            
            var damage = maximumDamage * intensity;
            target.TakeDamage(damage);
        }
        
        void ApplyPhysics(IDamageable target, float intensity, Vector3 projectileImpactPosition) {
            if(target is not MonoBehaviour targetMonoBehaviour) return;
    
            // CalculateForce
            var explosionForce = maximumExplosionForce * intensity;
            var explosionUpwardsModifier = maximumExplosionUpwardModifier * intensity;
            var impactDirection = (targetMonoBehaviour.transform.position - projectileImpactPosition).normalized;
            var totalForce = impactDirection * explosionForce + Vector3.up * explosionUpwardsModifier;
    
            // Player Controller uses own Physics System
            if (targetMonoBehaviour.TryGetComponent(out PlayerController playerController)) {
                playerController.ApplyExternalForce(totalForce);
                return;
            }
    
            // Normal Rigidbodies
            if (targetMonoBehaviour.TryGetComponent(out Rigidbody targetRigidbody)) {
                targetRigidbody.AddForce(impactDirection * explosionForce, ForceMode.Impulse);
                targetRigidbody.AddForce(Vector3.up * explosionUpwardsModifier, ForceMode.Impulse);
            }
        }
    }
}