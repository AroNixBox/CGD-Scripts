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
            var result = new ImpactResult {
                HitObjectOrigins = new List<(Transform, Vector3)>()
            };
            
            if (hazardPrefab == null) {
                Debug.LogWarning("HazardStrategy: No hazard prefab assigned!");
                return result;
            }

            Vector3 spawnPosition = impactData.Position + impactData.Normal * groundOffset;
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, impactData.Normal);

            var hazard = UnityEngine.Object.Instantiate(hazardPrefab, spawnPosition, spawnRotation);
            
            // Add hazard bounds points to hit origins (origin, top corners, elevated)
            foreach (var point in GetBoundsPoints(hazard)) {
                result.HitObjectOrigins.Add((hazard.transform, point));
            }

            if (additionalHazards != null) {
                foreach (var h in additionalHazards) {
                    var additionalHazard = UnityEngine.Object.Instantiate(h, spawnPosition, spawnRotation);
                    foreach (var point in GetBoundsPoints(additionalHazard)) {
                        result.HitObjectOrigins.Add((additionalHazard.transform, point));
                    }
                }
            }

            return result;
        }
        
        Vector3[] GetBoundsPoints(GameObject obj) {
            Bounds bounds;
            
            // Try Renderer first (visual bounds)
            if (obj.TryGetComponent<Renderer>(out var renderer)) {
                bounds = renderer.bounds;
            }
            // Fallback to Collider bounds
            else if (obj.TryGetComponent<Collider>(out var collider)) {
                bounds = collider.bounds;
            }
            // Check children for Renderer
            else {
                var childRenderer = obj.GetComponentInChildren<Renderer>();
                if (childRenderer != null) {
                    bounds = childRenderer.bounds;
                }
                else {
                    // Ultimate fallback: just use transform position + elevated point
                    return new[] { 
                        obj.transform.position,
                        obj.transform.position + Vector3.up * 3f
                    };
                }
            }
            
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 center = bounds.center;
            
            // Return: origin, top 4 corners only (no bottom corners), and elevated point
            return new[] {
                center,                                      // Origin/center
                new Vector3(min.x, max.y, min.z),           // Top corner 1
                new Vector3(min.x, max.y, max.z),           // Top corner 2
                new Vector3(max.x, max.y, min.z),           // Top corner 3
                new Vector3(max.x, max.y, max.z),           // Top corner 4
                center + Vector3.up * 3f                     // Elevated point
            };
        }
    }
}