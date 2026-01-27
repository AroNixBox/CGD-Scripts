using System;
using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Authority;
using Core.Runtime.Service;
using UnityEngine;

namespace Gameplay.Runtime {
    
    [RequireComponent(typeof(Collider))]
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
        
        [SerializeField] [Tooltip("Duration of the hazard in turns (of all players). Negative value means infinite lifetime, zero means it will be destroyed immediately before the next player turn.")]
        protected int turnDuration = -1; // Negative value means infinite lifetime
        
        protected Collider HazardCollider;
        protected AuthorityManager AuthorityManager;
        protected readonly Dictionary<AuthorityEntity, HazardData> EntitiesInHazard = new();
        protected int TurnCounter = 0;

        protected TerrainHeightWriter terrainHazardManager;
        private HazardType hazardType;

        public TerrainHeightWriter TerrainHazardManager { set => terrainHazardManager = value; }
        public HazardType HazardType { set => hazardType = value; }

        // 2.
        protected virtual void Start() {
            ValidateInheritance();
            if (!ServiceLocator.TryGet(out AuthorityManager))
                throw new NullReferenceException("Authority-Manager not registered");
            HazardCollider = GetComponent<Collider>();
            if (!HazardCollider.isTrigger) 
                throw new ArgumentException("Collider must be set as Trigger");

            OnEnable();
        }

        // 1.
        protected virtual void OnEnable() {
            if(!AuthorityManager) return;
            if(turnDuration >= 0) AuthorityManager.OnEntityAuthorityGained += CheckAlive;
        }

        
        protected virtual void OnDisable() {
            if (AuthorityManager == null) return;
            if(turnDuration >= 0) AuthorityManager.OnEntityAuthorityGained -= CheckAlive;
        }

        int ActualTurn => turnDuration * AuthorityManager.EntityCount;

        private void CheckAlive(AuthorityEntity _) {
            TurnCounter++;
            Debug.Log("Turncounter: " + TurnCounter + "/" + ActualTurn);
            if (TurnCounter <= ActualTurn) return;
            // turnDuration == 0 means survive one player turn
            if (turnDuration == 0 && TurnCounter == 1) return;
            if (terrainHazardManager != null) terrainHazardManager.ChangeTargets(hazardType, transform);
            Destroy(gameObject);
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