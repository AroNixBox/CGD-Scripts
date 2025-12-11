using System;
using System.Collections.Generic;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using Gameplay.Runtime.Interfaces;
using UnityEngine;

namespace Gameplay.Runtime
{
    public class TestHazard : MonoBehaviour
    {
        [SerializeField] private int applyCount;
        [SerializeField] private int damage;
        private static readonly Vector3 HazardBounds = new(1, 1, 1);
        private readonly Dictionary<AuthorityEntity, int> _authEntitiesInHazard = new();
        private AuthorityManager _authorityManager;

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _authorityManager))
                throw new NullReferenceException("Authority-Manager not registered");
            
            _authorityManager.OnEntityAuthorityRevoked += HandleOnTurnEnd;
        }
        
        private void OnEnable()
        {
            if (_authorityManager == null) return;
            _authorityManager.OnEntityAuthorityRevoked += HandleOnTurnEnd;
            _authorityManager.OnEntityAuthorityGained += HandleOnTurnStart;
        }

        private void OnDisable()
        {
            if (_authorityManager == null) return;
            _authorityManager.OnEntityAuthorityRevoked -= HandleOnTurnEnd;
            _authorityManager.OnEntityAuthorityGained -= HandleOnTurnStart;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(transform.position, HazardBounds);
        }

        private void HandleOnTurnEnd(AuthorityEntity newTurnEntity)
        {
            Collider[] hits = Physics.OverlapBox(transform.position, HazardBounds / 2);
            if (hits.Length == 0)
            {
                // Release this entity
                if (_authEntitiesInHazard.ContainsKey(newTurnEntity))
                    _authEntitiesInHazard.Remove(newTurnEntity);
                return;
            }

            List<AuthorityEntity> candidates = new();
            foreach (var hit in hits)
            {
                // No Authority inside Hazard Box
                if (!hit.TryGetComponent(out AuthorityEntity authorityEntity)) continue;
                candidates.Add(authorityEntity);
            }

            // Authority Entity from Event in Hazard Box?
            if (!candidates.Contains(newTurnEntity)) {
                _authEntitiesInHazard.Remove(newTurnEntity);
                return;
            }

            // Authority Entity in Hazard registered
            if (!_authEntitiesInHazard.TryAdd(newTurnEntity, 1))
            {
                _authEntitiesInHazard[newTurnEntity]++;

                // ApplyCount reached, apply damage
                if (_authEntitiesInHazard[newTurnEntity] < applyCount) return;
                ApplyEffect(newTurnEntity);
                _authEntitiesInHazard[newTurnEntity] = 0; // Reset Counter
            }
            else {
                // Entity was in hazard for the first turn

                // applyCount = 1, damage instant
                if (applyCount > 1) return;
                ApplyEffect(newTurnEntity);
                _authEntitiesInHazard[newTurnEntity] = 0;
            }
        }
        
        private void HandleOnTurnStart(AuthorityEntity newTurnEntity)
        {
            // You can also apply damage on turn start if needed
        }

        private void ApplyEffect(AuthorityEntity authorityEntity)
        {
            if (!authorityEntity.TryGetComponent(out IDamageable damageable)) return;
            Debug.Log($"Dealing Damage to {authorityEntity.name}");
            damageable.TakeDamage(20);
        }
    }
}