using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    /// <summary>
    /// Result data from a projectile impact
    /// </summary>
    public struct ImpactResult {
        public float TotalDamageDealt;
        public float TotalKnockbackApplied;
        public int TargetsHit;
    }

    public interface IImpactStrategy {
        public ImpactResult OnImpact(Vector3 impactPosition);
    }
}