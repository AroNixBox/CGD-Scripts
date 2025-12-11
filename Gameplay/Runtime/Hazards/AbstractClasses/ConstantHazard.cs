using Core.Runtime.Authority;
using UnityEngine;

namespace Gameplay.Runtime
{
    public abstract class ConstantHazard : Hazard
    {
        protected override void OnTriggerEnter(Collider other) {
            base.OnTriggerEnter(other);
            if (!other.gameObject.TryGetComponent(out AuthorityEntity authorityEntity)) return;
            if (!EntitiesInHazard.TryGetValue(authorityEntity, out var hazardData)) return;
            EnterEffect(other.gameObject, hazardData);
        }
        
        protected override void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.TryGetComponent(out AuthorityEntity authorityEntity)) return;
            if (!EntitiesInHazard.TryGetValue(authorityEntity, out var hazardData)) return;
            ExitEffect(other.gameObject, hazardData);
            base.OnTriggerExit(other);
        }
        
        protected abstract void EnterEffect(GameObject target, HazardData hazardData);
        
        protected abstract void ExitEffect(GameObject target, HazardData hazardData);
    }
}
