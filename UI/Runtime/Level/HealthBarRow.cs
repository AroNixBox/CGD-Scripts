using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarRow : MonoBehaviour {
        // Prefab
        [SerializeField, Required] HealthBarController healthBarPrefab;
        [SerializeField, Required] uint allowedHealthBarsPerRow = 2;
        public bool HasFreeSlot() => _spawnedHealthBars < allowedHealthBarsPerRow;

        // Map Healthbar to its owner
        int _spawnedHealthBars;

        public void InitializeHealthBar(IDamageable owner) {
            if (_spawnedHealthBars >= allowedHealthBarsPerRow) 
                Debug.LogWarning("Exceeded allowed healthbars per row, will still allow spawning a new one", transform);

            var healthBar = Instantiate(healthBarPrefab, transform);
            healthBar.Init(owner);

            _spawnedHealthBars++;
        }
        
    }
}
