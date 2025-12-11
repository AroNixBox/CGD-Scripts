using Core.Runtime.Authority;
using UnityEngine;

namespace Gameplay.Runtime
{
    public abstract class TurnBasedHazard : Hazard
    {
        protected enum TurnState {
            TurnStart,
            TurnEnd 
        }
        
        [SerializeField] protected bool triggerOnTurnStart = true;
        [SerializeField] protected bool triggerOnTurnEnd = false;
        
        private bool _initialized;
        
        protected override void Start() {
            base.Start();
            _initialized = true;
            OnEnable();
        }
        
        protected virtual void OnEnable() {
            if (!_initialized) return;
            AuthorityManager.OnEntityAuthorityGained += HandleOnTurnStartInternal;
            AuthorityManager.OnEntityAuthorityRevoked += HandleOnTurnEndInternal;
        }
        
        protected virtual void OnDisable() {
            AuthorityManager.OnEntityAuthorityGained -= HandleOnTurnStartInternal;
            AuthorityManager.OnEntityAuthorityRevoked -= HandleOnTurnEndInternal;
        }
        
        private void HandleOnTurnStartInternal(AuthorityEntity newTurnEntity) {
            if (!EntitiesInHazard.TryGetValue(newTurnEntity, out var hazardData)) return;
            hazardData.TurnCount++;
            HandleOnTurnStart(newTurnEntity, hazardData);
        }
        
        protected void HandleOnTurnStart(AuthorityEntity newTurnEntity, HazardData hazardData) {
            if(triggerOnTurnStart) TriggerEffect(newTurnEntity.gameObject, hazardData, TurnState.TurnStart);
        }
        
        private void HandleOnTurnEndInternal(AuthorityEntity newTurnEntity) {
            if (!EntitiesInHazard.TryGetValue(newTurnEntity, out var hazardData)) return;
            HandleOnTurnEnd(newTurnEntity, hazardData);
        }

        protected void HandleOnTurnEnd(AuthorityEntity newTurnEntity, HazardData hazardData) {
            if(triggerOnTurnEnd) TriggerEffect(newTurnEntity.gameObject, hazardData, TurnState.TurnEnd);
        }

        protected abstract void TriggerEffect(GameObject target, HazardData hazardData, TurnState turnState);
    }
}
