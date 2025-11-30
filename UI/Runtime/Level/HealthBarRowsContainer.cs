using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Authority;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarRowsContainer : MonoBehaviour {
        [SerializeField, Required] HealthBarRow rowPrefab;
        readonly List<HealthBarRow> _healthBarRows = new();

        void OnEnable() {
            AuthorityManager.OnEntitySpawned += TryCreateHealthBar;
        }

        void OnDisable() {
            AuthorityManager.OnEntitySpawned -= TryCreateHealthBar;
        }

        void TryCreateHealthBar(AuthorityEntity authEntity) {
            if (!authEntity.TryGetComponent(out IDamageable damageable)) return;
            CreateHealthBar(damageable);
        }

        // TODO: Event that listens to some Creation Event that spawns healthbars
        void CreateHealthBar(IDamageable owner) {
            // Available Health Bar Row?
            // Get last health bar entry
            var lastRow = _healthBarRows.LastOrDefault();
            // No Last entry OR row exceeds max healthBars, create a new one
            if (lastRow == null  || !lastRow.HasFreeSlot()) { 
                lastRow = Instantiate(rowPrefab, transform);
                _healthBarRows.Add(lastRow);
            }
            
            lastRow.InitializeHealthBar(owner);
        }
    }
}