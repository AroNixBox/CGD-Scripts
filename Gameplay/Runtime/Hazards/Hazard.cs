using System;
using System.Collections.Generic;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using UnityEngine;

namespace Gameplay.Runtime {
    public abstract class Hazard : MonoBehaviour {
        protected class HazardData {
            public int TurnCount;
            
            public HazardData(int turnCount = 0) {
                TurnCount = turnCount;
            }
        }

        [SerializeField] private Collider hazardCollider;
        [SerializeField] protected bool triggerOnTurnStart = true;
        [SerializeField] protected bool triggerOnTurnEnd = false;
        private AuthorityManager _authorityManager;
        private readonly Dictionary<AuthorityEntity, HazardData> _entitiesInHazard = new();
        
        private bool _initialized;


        protected virtual void Start() {
            if (hazardCollider == null)
                throw new NullReferenceException("HazardCollider not Set");
            if (!ServiceLocator.TryGet(out _authorityManager))
                throw new NullReferenceException("Authority-Manager not registered");
            if (!hazardCollider.isTrigger) 
                throw new ArgumentException("Hazard Collider must be set as Trigger");
            _initialized = true;
            print("hallo");
            OnEnable();
        }

        protected virtual void OnEnable() {
            if (!_initialized) return;
            _authorityManager.OnEntityAuthorityGained += HandleOnTurnStartInternal;
            _authorityManager.OnEntityAuthorityRevoked += HandleOnTurnEndInternal;
        }
        
        protected virtual void OnDisable() {
            _authorityManager.OnEntityAuthorityGained -= HandleOnTurnStartInternal;
            _authorityManager.OnEntityAuthorityRevoked -= HandleOnTurnEndInternal;
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent(out AuthorityEntity authorityEntity)) return;
            if (_entitiesInHazard.ContainsKey(authorityEntity)) return;
            _entitiesInHazard.Add(authorityEntity, new HazardData());
        }
        
        protected virtual void OnTriggerExit(Collider other) {
            if (!other.TryGetComponent(out AuthorityEntity authorityEntity)) return;
            _entitiesInHazard.Remove(authorityEntity);
        }

        private void HandleOnTurnStartInternal(AuthorityEntity newTurnEntity) {
            if (!_entitiesInHazard.TryGetValue(newTurnEntity, out var hazardData)) return;
            hazardData.TurnCount++;
            HandleOnTurnStart(newTurnEntity, hazardData);
        }
        
        protected virtual void HandleOnTurnStart(AuthorityEntity newTurnEntity, HazardData hazardData) {
            if(triggerOnTurnStart) TriggerEffect(newTurnEntity.gameObject, hazardData);
        }
        
        private void HandleOnTurnEndInternal(AuthorityEntity newTurnEntity) {
            if (!_entitiesInHazard.TryGetValue(newTurnEntity, out var hazardData)) return;
            HandleOnTurnEnd(newTurnEntity, hazardData);
        }

        protected virtual void HandleOnTurnEnd(AuthorityEntity newTurnEntity, HazardData hazardData) {
            if(triggerOnTurnEnd) TriggerEffect(newTurnEntity.gameObject, hazardData);
        }

        protected abstract void TriggerEffect(GameObject target, HazardData hazardData);
    }
}