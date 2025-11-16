using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Runtime.Player.Combat {
    [CreateAssetMenu(menuName = "Player/Combat/Projectile Impact Data")]
    public class ProjectileImpactData : ScriptableObject {
        [Header("Settings")]
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

        [SerializeReference, SerializeField][InlineProperty, HideLabel, BoxGroup("Impact Strategy")] public IImpactStrategy impactStrategy;
        public IImpactStrategy GetImpactStrategy() => impactStrategy;

        [Header("Effects")]
        [InfoBox("No Effects? Leave Fields empty")]
        [SerializeField] GameObject impactVfx;
        public GameObject GetImpactVfx() => impactVfx;
        [SerializeField] AudioClip impactSfx;
        public AudioClip GetImpactSfx() => impactSfx;

        // ProjectileAction = Damage, Apply Force, VFX & Sfx
    }
}