using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    /// <summary>
    /// Result data from a projectile impact
    /// </summary>
    public struct ImpactResult {
        public float TotalDamageDealt;
        public float TotalKnockbackApplied;
        public int TargetsHit;
        public List<Vector3> HitObjectOrigins;
    }

    /// <summary>
    /// Data passed to impact strategies containing collision information
    /// </summary>
    public struct ImpactData {
        public Vector3 Position;
        public Vector3 Normal;
        
        public static ImpactData FromPosition(Vector3 position) => new ImpactData {
            Position = position,
            Normal = Vector3.up
        };
        
        public static ImpactData FromCollision(Collision collision) {
            var contact = collision.GetContact(0);
            return new ImpactData {
                Position = contact.point,
                Normal = contact.normal
            };
        }
    }

    public interface IImpactStrategy {
        public ImpactResult OnImpact(Vector3 impactPosition);
        public ImpactResult OnImpact(ImpactData impactData) => OnImpact(impactData.Position);
    }
}