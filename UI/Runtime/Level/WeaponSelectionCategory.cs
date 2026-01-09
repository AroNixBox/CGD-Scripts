using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Runtime.Player.Combat;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Runtime.Level {
    public class WeaponSelectionCategory : MonoBehaviour {
        [SerializeField, Required] Image outline;
        
        readonly List<WeaponSelectionEntry> _entries = new();
        RectTransform _rect;
        public RectTransform RectTransform => _rect;

        void Awake() {
            _rect = GetComponent<RectTransform>();
        }

        public void EnableOutline() => outline.gameObject.SetActive(true);
        public void DisableOutline() => outline.gameObject.SetActive(false);

        public void AddEntry(WeaponSelectionEntry entryPrefab, WeaponData data) {
            var entry = Instantiate(entryPrefab, transform);
            entry.Init(data);
            _entries.Add(entry);
            
            // New element at second sibling, first is always the outline and
            // the siblings need to be arranged in reverse, due to the first entry being 
            // index 0 in the stash
            entry.transform.SetSiblingIndex(1);
        }

        public void SelectEntry(WeaponData data) {
            var entry = _entries.FirstOrDefault(e => e.Data == data);
            if (entry == null) return;
            
            // Last one is active
            entry.transform.SetAsLastSibling();
        }

        public void UpdateAmmo(WeaponData data, int amount) {
            var entry = _entries.FirstOrDefault(e => e.Data == data);
            if (entry != null) {
                entry.SetAmmo(amount);
            }
        }
    }
}
