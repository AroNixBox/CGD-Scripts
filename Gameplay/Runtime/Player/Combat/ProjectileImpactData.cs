using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Impact Data")]
    public class ProjectileImpactData : ScriptableObject {
        [SerializeField] uint aoeRadius = 1;
        public uint GetAOERadius() => aoeRadius;
        
        [Tooltip("Impacts Damage & Physics")] 
        [SerializeField] AnimationCurve dropOffCurve;
        public AnimationCurve GetDropOffCurve() => dropOffCurve;
        
        [EnumToggleButtons][SerializeField] 
        EProjectileActionTrigger projectileActionTrigger;
        public EProjectileActionTrigger GetProjectileActionTrigger() => projectileActionTrigger;
        public enum EProjectileActionTrigger {
            FirstCollision,
            Countdown,
            // TODO:
            // Triggered when the Projectile collides with a Damageable
            // FirstCollisionWithDamageable
        }

        [ShowIf("@projectileActionTrigger == EProjectileActionTrigger.Countdown")]
        [SerializeField] int projectileActionCountdown;
        public int GetProjectileActionCountdown() {
            if (projectileActionTrigger != EProjectileActionTrigger.Countdown) {
                throw new NotSupportedException(
                    $"Cant get Countdown for Projectile Action, because {projectileActionTrigger} is selected in this Projectile Data");
            }

            return projectileActionCountdown;
        }
    
        // TODO:
        // [Header("Physical Impact")]
        // [field: SerializeField] public float ExplosionForce { get; private set; }
        // [field: SerializeField] public float ExplosionUpwardModifier { get; private set; }

        [SerializeField] float maximumDamage;
        public float GetMaximumDamage() => maximumDamage;

        // ProjectileAction = Damage, Apply Force, VFX & Sfx

        // TODO: Implement
        // [field: SerializeField] public float DropoffBeginDistance { get; private set; }
        // [field: SerializeField] public float DropoffEndDistance { get; private set; }
        // [field: SerializeField] public float DistanceMultiplierValue { get; private set; }
    }
}