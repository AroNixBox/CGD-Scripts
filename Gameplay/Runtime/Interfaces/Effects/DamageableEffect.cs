using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Runtime.Interfaces.Effects {
    public class DamageThresholdEventInvoker : MonoBehaviour {
        [SerializeField] List<EffectMapping> effectMapping;
        readonly Dictionary<EffectMapping, int> _mappingsApplyCount = new();
        [SerializeField, Required, ValidateInput(nameof(ValidateDamageable), "Reference needs to implement IDamageable", InfoMessageType.Error)]
        MonoBehaviour damageable;
        IDamageable _damageable;
        bool ValidateDamageable(MonoBehaviour mb) => mb != null && mb is IDamageable;
        
        void Awake() {
            _damageable = damageable as IDamageable;

            foreach (var tm in effectMapping) {
                _mappingsApplyCount[tm] = 0;
            }
        }
        
        void OnEnable() {
            if (_damageable != null) _damageable.OnCurrentHealthChanged += FireEvents;
        }

        void OnDisable() {
            if (_damageable != null) _damageable.OnCurrentHealthChanged -= FireEvents;
        }

        void FireEvents(float currentHealth) {
            if (effectMapping == null) return;
            
            // Select all relevant Mappings that are affected by the threshold
            var relevantMappings = effectMapping
                .Where(m => _mappingsApplyCount[m] < m.maxApplyCount)
                .Where(entry => currentHealth <= entry.healthThreshold.y && currentHealth >= entry.healthThreshold.x)
                .ToList();

            if (relevantMappings.Any(e => e.exclusive)) {
                // Fire on first matching exclusive mapping if there is one
                // All exclusive effects are playing together
                foreach (var r in relevantMappings) {
                    r.evt?.Invoke();
                    _mappingsApplyCount[r] += 1;
                }
                
                // Dont fire any low prio effects
                return;
            }

            // Fire on all matching mappings
            foreach (var r in relevantMappings) {
                r.evt?.Invoke();
                _mappingsApplyCount[r] += 1;
            }
        }
        
        [Serializable]
        class EffectMapping {
            public UnityEvent evt;
            public int maxApplyCount;
            [Tooltip("Suppresses other effects which are not exclusive")]
            public bool exclusive;
            [MinMaxSlider(0f, 100f, true)]
            [Tooltip("Select Start (x) & Endvalue (y) der of the health range")]
            public Vector2 healthThreshold;            
        }
    }
}