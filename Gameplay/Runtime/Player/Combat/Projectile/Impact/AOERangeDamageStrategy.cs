using System;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    /// <summary>
    /// Extension of AOEDamageStrategy that scales damage based on the distance
    /// from the shooter to the impact point.
    /// </summary>
    [Serializable]
    public class AOERangeDamageStrategy : AOEDamageStrategy {
        [Header("Range-Based Damage")]
        [Tooltip("Maximum distance for the range damage ramp-up effect.")]
        [SerializeField] float maxRangeRampUp = 50f;
        
        [Tooltip("Curve to evaluate damage multiplier based on distance from shooter to impact. X-axis is normalized distance (0-1), Y-axis is damage multiplier.")]
        [SerializeField] AnimationCurve rangeRampUpCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);

        Vector3 _shooterPosition;

        /// <summary>
        /// Sets the shooter's position for range-based damage calculation.
        /// Must be called before OnImpact.
        /// </summary>
        public void SetShooterPosition(Vector3 position) {
            _shooterPosition = position;
        }

        public override ImpactResult OnImpact(Vector3 impactPosition) {
            var result = base.OnImpact(impactPosition);
            
            // Calculate range multiplier based on distance from shooter to impact
            float distanceFromShooter = Vector3.Distance(_shooterPosition, impactPosition);
            float normalizedDistance = Mathf.Clamp01(distanceFromShooter / maxRangeRampUp);
            float rangeMultiplier = rangeRampUpCurve.Evaluate(normalizedDistance);
            
            // Apply range multiplier to the damage dealt
            result.TotalDamageDealt *= rangeMultiplier;
            
            return result;
        }
    }
}
