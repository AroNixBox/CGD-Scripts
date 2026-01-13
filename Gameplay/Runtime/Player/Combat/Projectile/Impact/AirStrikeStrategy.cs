using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    /// <summary>
    /// Impact strategy that spawns a separate projectile from above the impact position,
    /// simulating an air strike effect.
    /// </summary>
    [Serializable]
    public class AirStrikeStrategy : IImpactStrategy {
        [Header("Air Strike Configuration")]
        [Tooltip("Height above impact position where the air strike projectile spawns.")]
        [SerializeField] float spawnHeight = 20f;

        [Tooltip("Prefab for the projectile that will be dropped from above.")]
        [SerializeField] Projectile airStrikeProjectilePrefab;

        [Tooltip("Impact data for the spawned air strike projectile. Defines behavior on impact.")]
        [SerializeField] ProjectileImpactData airStrikeImpactData;

        [Tooltip("Optional delay before spawning the air strike projectile.")]
        [SerializeField] float spawnDelay = 0f;

        [Header("Projectile Physics")]
        [Tooltip("Initial downward velocity of the air strike projectile.")]
        [SerializeField] float initialDownwardForce = 10f;

        [Tooltip("Mass of the air strike projectile.")]
        [SerializeField] float projectileMass = 1f;

        [Tooltip("Drag of the air strike projectile.")]
        [SerializeField] float projectileDrag = 0f;
        

        public ImpactResult OnImpact(Vector3 impactPosition) {
            var result = new ImpactResult {
                HitObjectOrigins = new List<(Transform, Vector3)>(),
                TotalDamageDealt = 0,
                TotalKnockbackApplied = 0,
                TargetsHit = 0
            };

            if (airStrikeProjectilePrefab == null) {
                Debug.LogWarning("AirStrikeStrategy: No projectile prefab assigned!");
                return result;
            }

            if (airStrikeImpactData == null) {
                Debug.LogWarning("AirStrikeStrategy: No impact data assigned for air strike projectile!");
                return result;
            }

            // Calculate spawn position above the impact
            var spawnPosition = impactPosition + Vector3.up * spawnHeight;

            // Create transforms along the predicted flight path
            CreateFlightPathTransforms(spawnPosition, impactPosition, result);

            if (spawnDelay > 0f) {
                // Use a coroutine runner to delay spawn
                DelayedSpawn(spawnPosition);
            }
            else {
                SpawnAirStrikeProjectile(spawnPosition);
            }

            return result;
        }

        void CreateFlightPathTransforms(Vector3 start, Vector3 end, ImpactResult result) {
            // Create a parent to hold all path point transforms
            var pathParent = new GameObject("AirStrike_FlightPath");

            int pathPointCount = 5;
                
            for (int i = 0; i <= pathPointCount; i++) {
                float t = (float)i / pathPointCount;
                Vector3 pointPosition = Vector3.Lerp(start, end, t);
                
                var pathPoint = new GameObject($"FlightPath_Point_{i}");
                pathPoint.transform.position = pointPosition;
                pathPoint.transform.SetParent(pathParent.transform);
                
                result.HitObjectOrigins.Add((pathPoint.transform, pointPosition));
            }
        }

        void SpawnAirStrikeProjectile(Vector3 spawnPosition) {
            // Instantiate the air strike projectile
            var projectileInstance = UnityEngine.Object.Instantiate(
                airStrikeProjectilePrefab,
                spawnPosition,
                Quaternion.LookRotation(Vector3.down)
            );

            // Initialize the projectile to fall downward
            projectileInstance.Init(
                projectileMass,
                projectileDrag,
                initialDownwardForce,
                Vector3.down,
                airStrikeImpactData
            );
        }

        void DelayedSpawn(Vector3 spawnPosition) {
            // Create a temporary GameObject to handle the delayed spawn via coroutine
            var delayHelper = new GameObject("AirStrike_DelayHelper");
            var delayComponent = delayHelper.AddComponent<AirStrikeDelayHelper>();
            delayComponent.Initialize(this, spawnPosition, spawnDelay);
        }

        // Called by delay helper to spawn after delay
        internal void ExecuteDelayedSpawn(Vector3 spawnPosition) {
            SpawnAirStrikeProjectile(spawnPosition);
        }
    }

    /// <summary>
    /// Helper component to handle delayed spawning of air strike projectiles.
    /// </summary>
    internal class AirStrikeDelayHelper : MonoBehaviour {
        AirStrikeStrategy _strategy;
        Vector3 _spawnPosition;
        float _delay;

        public void Initialize(AirStrikeStrategy strategy, Vector3 spawnPosition, float delay) {
            _strategy = strategy;
            _spawnPosition = spawnPosition;
            _delay = delay;
            StartCoroutine(DelayedSpawnCoroutine());
        }

        System.Collections.IEnumerator DelayedSpawnCoroutine() {
            yield return new WaitForSeconds(_delay);
            _strategy.ExecuteDelayedSpawn(_spawnPosition);
            Destroy(gameObject);
        }
    }
}

