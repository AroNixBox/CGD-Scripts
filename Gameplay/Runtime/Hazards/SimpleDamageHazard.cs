using Core.Runtime.Authority;
using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime
{
    public class SimpleDamageHazard : Hazard
    {
        [SerializeField] private int damagePerTurn = 10;
        
        protected override void TriggerEffect(GameObject target, HazardData hazardData)
        {
            if (!target.TryGetComponent(out IDamageable damageable)) return;
            damageable.TakeDamage(damagePerTurn);
            print($"{gameObject.name} applied {damagePerTurn} damage to {target.name}.");
        }
    }
}
