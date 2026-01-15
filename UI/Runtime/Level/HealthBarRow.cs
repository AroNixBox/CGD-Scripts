using System;
using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Backend;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarRow : MonoBehaviour {
        // Prefab
        [SerializeField, Required] HealthBarController healthBarPrefab;
        [SerializeField, Required] uint allowedHealthBarsPerRow = 2;
        public bool HasFreeSlot() => _spawnedHealthBars.Count < allowedHealthBarsPerRow;
        public bool ContainsHealthBarOfOwner(IDamageable owner) => _spawnedHealthBars.ContainsKey(owner);

        // Map Healthbar to its owner
        readonly Dictionary<IDamageable, HealthBarController> _spawnedHealthBars = new();

        public void InitializeHealthBar(IDamageable owner, UserData userData) {
            if (_spawnedHealthBars.Count >= allowedHealthBarsPerRow) 
                Debug.LogWarning("Exceeded allowed healthbars per row, will still allow spawning a new one", transform);

            if (_spawnedHealthBars.ContainsKey(owner))
                throw new NotSupportedException("Tried adding a second healthbar of the same Owner to this row");

            var healthBar = Instantiate(healthBarPrefab, transform);
            healthBar.Init(owner, userData);
            
            _spawnedHealthBars.Add(owner, healthBar);
        }
        
        public int GetHealthBarCount() => _spawnedHealthBars.Count;
        
        public (IDamageable owner, UserData userData) GetFirstHealthBar() {
            var first = _spawnedHealthBars.First();
            return (first.Key, first.Value.UserData);
        }

        public void RemoveHealthBar(IDamageable owner) {
            if (!_spawnedHealthBars.ContainsKey(owner))
                throw new NotSupportedException("Tried removing a Healthbar that is not from this owner");

            var healthBar = _spawnedHealthBars[owner];
            _spawnedHealthBars.Remove(owner);
            if(healthBar != null)
                Destroy(healthBar.gameObject);
        }
    }
}
