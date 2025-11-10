using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Impact Data")]
    public class ProjectileImpactData : ScriptableObject {
        [field: SerializeField] public float DamageValue { get; private set; }
        [field: SerializeField] public float AOERange { get; private set; }
    
        // TODO:
        // [Header("Physical Impact")]
        // [field: SerializeField] public float ExplosionForce { get; private set; }
        // [field: SerializeField] public float ExplosionRadius { get; private set; }
        // [field: SerializeField] public float ExplosionUpwardModifier { get; private set; }
        
        [field: SerializeField] public AnimationCurve DropOffCurve { get; private set; }
        [field: SerializeField] public float MaximumDropoffDamage { get; private set; }
        [field: SerializeField] public float DropoffBeginDistance { get; private set; }
        [field: SerializeField] public float DropoffEndDistance { get; private set; }
        [field: SerializeField] public float DistanceMultiplierValue { get; private set; }
    }
}