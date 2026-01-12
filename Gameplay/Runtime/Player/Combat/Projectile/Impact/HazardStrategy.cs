using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    [Serializable]
    public class HazardStrategy : IImpactStrategy {
        [SerializeField, AssetsOnly, Required] 
        GameObject hazardPrefab;

        [SerializeField] List<GameObject> additionalHazards;

        [SerializeField, Tooltip("Offset from ground surface")]
        float groundOffset = 0.01f;

        public ImpactResult OnImpact(Vector3 impactPosition) {
            return OnImpact(ImpactData.FromPosition(impactPosition));
        }
        
        public ImpactResult OnImpact(ImpactData impactData) {
            var result = new ImpactResult();
            
            if (hazardPrefab == null) {
                Debug.LogWarning("HazardStrategy: No hazard prefab assigned!");
                return result;
            }

            Vector3 spawnPosition = impactData.Position + impactData.Normal * groundOffset;
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, impactData.Normal);

            var hazard = UnityEngine.Object.Instantiate(hazardPrefab, spawnPosition, spawnRotation);

            if (additionalHazards != null) {
                foreach (var h in additionalHazards) {
                    UnityEngine.Object.Instantiate(h, spawnPosition, spawnRotation);
                }
            }
            // TODO: Could ask for all objects that are affected by hazard, they could be added to ttargetgroup then

            return result;
        }
    }
}