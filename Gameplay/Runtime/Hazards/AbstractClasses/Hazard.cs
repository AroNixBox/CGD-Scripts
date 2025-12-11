using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Type[] _allowedTypes = {
            typeof(TurnBasedHazard),
            typeof(ConstantHazard),
        };

        [SerializeField] protected Collider hazardCollider;
        protected AuthorityManager AuthorityManager;
        protected readonly Dictionary<AuthorityEntity, HazardData> EntitiesInHazard = new();
        

        protected virtual void Start() {
            ValidateInheritance();
            if (hazardCollider == null)
                throw new NullReferenceException("HazardCollider not Set");
            if (!ServiceLocator.TryGet(out AuthorityManager))
                throw new NullReferenceException("Authority-Manager not registered");
            if (!hazardCollider.isTrigger) 
                throw new ArgumentException("Hazard Collider must be set as Trigger");
        }
        
        private void ValidateInheritance() {
            Type actualType = GetType();
            if (_allowedTypes.Any(allowed => allowed.IsAssignableFrom(actualType))) return;

            throw new InvalidOperationException(
                $"Type '{actualType.Name}' is not allowed to inherit from Hazard." +
                $"Please inherit from: {string.Join(", ", Array.ConvertAll(_allowedTypes, t => t.Name))}");
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (!other.TryGetComponent(out AuthorityEntity authorityEntity)) return;
            if (EntitiesInHazard.ContainsKey(authorityEntity)) return;
            EntitiesInHazard.Add(authorityEntity, new HazardData());
        }
        
        protected virtual void OnTriggerExit(Collider other) {
            if (!other.TryGetComponent(out AuthorityEntity authorityEntity)) return;
            EntitiesInHazard.Remove(authorityEntity);
        }

        
    }
}