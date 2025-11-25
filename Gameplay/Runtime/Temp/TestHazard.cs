using System;
using System.Collections.Generic;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime {
    public class TestHazard : MonoBehaviour {
        [SerializeField] int applyCount;
        [SerializeField] int damage;
        static readonly Vector3 HazardBounds = new(1, 1, 1);
        readonly Dictionary<AuthorityEntity, int> _authEntitiesInHazard = new();
        AuthorityManager _authorityManager;
        
        void Start() {
            if (!ServiceLocator.TryGet(out _authorityManager))
                throw new NullReferenceException("Authority-Manager aint registered");
            
            // Damage on Turn End
            _authorityManager.OnAuthorityAuthorityRevoked += CheckDamageablesInRange;
            // Damage on Turn Start
            // authorityManager.OnAuthorityAuthorityGained += CheckDamageablesInRange;
        }

        void OnDisable() {
            if(_authorityManager != null)
                _authorityManager.OnAuthorityAuthorityRevoked -= CheckDamageablesInRange;
            // authorityManager.OnAuthorityAuthorityGained -= CheckDamageablesInRange;
        }

        void CheckDamageablesInRange(AuthorityEntity newTurnEntity) {
            // 1 Check if Authority Entity is in Box
            var hits = Physics.OverlapBox(transform.position, HazardBounds / 2);
            if (hits.Length == 0) {
                // Release this entity
                if(_authEntitiesInHazard.ContainsKey(newTurnEntity))
                    _authEntitiesInHazard.Remove(newTurnEntity);
                return;
            }

            List<AuthorityEntity> candidates = new();
            foreach (var hit in hits) {
                // No Authority inside Hazard Box
                if (!hit.TryGetComponent(out AuthorityEntity authorityEntity)) continue;
                candidates.Add(authorityEntity);
            }

            // Authority Entity from Event in Hazard Box?
            if (!candidates.Contains(newTurnEntity)) {
                if(_authEntitiesInHazard.ContainsKey(newTurnEntity))
                    _authEntitiesInHazard.Remove(newTurnEntity);
                return;
            }
            
            // Authority Entity in Hazard registered
            if(_authEntitiesInHazard.ContainsKey(newTurnEntity)) {
                _authEntitiesInHazard[newTurnEntity]++;
                
                // ApplyCount reached, apply damage
                if(_authEntitiesInHazard[newTurnEntity] >= applyCount) {
                    ApplyEffect(newTurnEntity);
                    _authEntitiesInHazard[newTurnEntity] = 0; // Reset Counter
                }
            }
            else {
                // Entity was in hazard for the first turn
                _authEntitiesInHazard[newTurnEntity] = 1;
                
                // applyCount = 1, damage instant
                if(applyCount <= 1) {
                    ApplyEffect(newTurnEntity);
                    _authEntitiesInHazard[newTurnEntity] = 0;
                }
            }
        }
        void ApplyEffect(AuthorityEntity authorityEntity) {
            if (!authorityEntity.TryGetComponent(out IDamageable damageable)) return;
            Debug.Log($"Dealing Damage to {authorityEntity.name}");
            damageable.TakeDamage(20);
        }

        void OnDrawGizmos() {
            Gizmos.DrawCube(transform.position, HazardBounds);
        }
    }
}
