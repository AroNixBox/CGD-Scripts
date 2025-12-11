using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime
{
    public class SimpleDamageHazard : TurnBasedHazard
    {
        [SerializeField] private int damagePerTurn = 10;
        
        protected override void TriggerEffect(GameObject target, HazardData hazardData, TurnState turnState)
        {
            if (!target.TryGetComponent(out IDamageable damageable)) return;
            damageable.TakeDamage(damagePerTurn);
        }
    }
}
