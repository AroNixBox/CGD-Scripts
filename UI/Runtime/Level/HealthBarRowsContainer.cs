using System.Collections.Generic;
using System.Linq;
using Core.Runtime.Authority;
using Core.Runtime.Backend;
using Gameplay.Runtime.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Runtime.Level {
    public class HealthBarRowsContainer : MonoBehaviour {
        [SerializeField, Required] HealthBarRow rowPrefab;
        readonly List<HealthBarRow> _healthBarRows = new();

        void OnEnable() {
            AuthorityManager.OnEntitySpawned += TryCreateHealthBar;
            AuthorityManager.OnEntityDied += TryRemoveHealthBar;
        }

        void TryRemoveHealthBar(AuthorityEntity diedEntity) {
            if (!diedEntity.TryGetComponent(out IDamageable diedDamageable))
                return;
        
            int removedRowIndex = -1;
            // Find the Row
            foreach (var healthBarRow in _healthBarRows) {
                if (healthBarRow.ContainsHealthBarOfOwner(diedDamageable)) {
                    healthBarRow.RemoveHealthBar(diedDamageable);
                    removedRowIndex = _healthBarRows.IndexOf(healthBarRow);
                    break;
                }
            }
        
            if (removedRowIndex == -1) return;
        
            // Shift healthbars from subsequent rows if current row has free slots
            for (int i = removedRowIndex; i < _healthBarRows.Count - 1; i++) {
                var currentRow = _healthBarRows[i];
                var nextRow = _healthBarRows[i + 1];

                // While current row has free slots and next row has healthbars
                while (currentRow.HasFreeSlot() && nextRow.GetHealthBarCount() > 0) {
                    var (owner, userData) = nextRow.GetFirstHealthBar();
                    nextRow.RemoveHealthBar(owner);
                    currentRow.InitializeHealthBar(owner, userData);
                }

                // If next row is empty, remove it
                if (nextRow.GetHealthBarCount() == 0) {
                    _healthBarRows.RemoveAt(i + 1);
                    Destroy(nextRow.gameObject);
                    i--; // Decrement to check the same index again with the next row
                }
            }

        }

        void OnDisable() {
            AuthorityManager.OnEntitySpawned -= TryCreateHealthBar;
        }

        void TryCreateHealthBar(AuthorityEntity authEntity, UserData userData) {
            if (!authEntity.TryGetComponent(out IDamageable damageable)) return;
            CreateHealthBar(damageable, userData);
        }

        // TODO: Event that listens to some Creation Event that spawns healthbars
        void CreateHealthBar(IDamageable owner, UserData userData) {
            // Available Health Bar Row?
            // Get last health bar entry
            var lastRow = _healthBarRows.LastOrDefault();
            // No Last entry OR row exceeds max healthBars, create a new one
            if (lastRow == null  || !lastRow.HasFreeSlot()) { 
                lastRow = Instantiate(rowPrefab, transform);
                _healthBarRows.Add(lastRow);
            }
            
            lastRow.InitializeHealthBar(owner, userData);
        }
    }
}