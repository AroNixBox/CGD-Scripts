using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Runtime.Player.Combat {
    /// <summary>
    /// Impact strategy that spawns a plane flying over the impact position,
    /// which drops a projectile when directly above the target.
    /// </summary>
    [Serializable]
    public class AirStrikeStrategy : IImpactStrategy {
        [Header("Plane Configuration")]
        [Tooltip("Prefab for the plane that flies over and drops the projectile.")]
        [SerializeField] GameObject planePrefab;

        [Tooltip("Speed at which the plane flies.")]
        [SerializeField] float planeSpeed = 30f;

        [Tooltip("Height at which the plane flies above the impact position.")]
        [SerializeField] float flyHeight = 20f;

        [Tooltip("Distance from the drop point where the plane spawns and despawns.")]
        [SerializeField] float planeFlightDistance = 50f;

        [Header("Projectile Configuration")]
        [Tooltip("Prefab for the projectile that will be dropped from the plane.")]
        [SerializeField] Projectile airStrikeProjectilePrefab;

        [Tooltip("Impact data for the spawned air strike projectile. Defines behavior on impact.")]
        [SerializeField] ProjectileImpactData airStrikeImpactData;

        [Header("Projectile Physics")]
        [Tooltip("Initial downward velocity of the air strike projectile.")]
        [SerializeField] float initialDownwardForce = 10f;

        [Tooltip("Mass of the air strike projectile.")]
        [SerializeField] float projectileMass = 1f;

        [Tooltip("Drag of the air strike projectile.")]
        [SerializeField] float projectileDrag;

        public ImpactResult OnImpact(Vector3 impactPosition) {
            var result = new ImpactResult {
                HitObjectOrigins = new List<Vector3>(),
                TotalDamageDealt = 0,
                TotalKnockbackApplied = 0,
                TargetsHit = 0
            };

            if (planePrefab == null) {
                Debug.LogWarning("AirStrikeStrategy: No plane prefab assigned!");
                return result;
            }

            if (airStrikeProjectilePrefab == null) {
                Debug.LogWarning("AirStrikeStrategy: No projectile prefab assigned!");
                return result;
            }

            if (airStrikeImpactData == null) {
                Debug.LogWarning("AirStrikeStrategy: No impact data assigned for air strike projectile!");
                return result;
            }

            // Calculate the drop position (directly above impact)
            var dropPosition = impactPosition + Vector3.up * flyHeight;

            // Spawn plane and let it fly over
            SpawnPlane(dropPosition);

            var extendedImpactPosition = impactPosition + Vector3.down * 3;
            result.HitObjectOrigins.Add(extendedImpactPosition);
            result.HitObjectOrigins.Add(dropPosition);

            return result;
        }
        
        /// <returns>The Transforms that should be observed by the Camera</returns>
        void SpawnPlane(Vector3 dropPosition) {
            // Plane starts from one side and flies to the other
            var startPosition = dropPosition + Vector3.forward * planeFlightDistance;
            var endPosition = dropPosition - Vector3.forward * planeFlightDistance;

            // Spawn the plane
            var planeInstance = UnityEngine.Object.Instantiate(
                planePrefab,
                startPosition,
                Quaternion.LookRotation(-Vector3.forward) // Look towards flight direction
            );

            // Add the flight controller component
            var flightController = planeInstance.AddComponent<AirStrikePlaneController>();
            flightController.Initialize(
                this,
                dropPosition,
                endPosition,
                planeSpeed
            );
        }

        internal void SpawnProjectile(Vector3 spawnPosition) {
            // Instantiate the air strike projectile
            var projectileInstance = UnityEngine.Object.Instantiate(
                airStrikeProjectilePrefab,
                spawnPosition,
                Quaternion.identity
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
    }

    /// <summary>
    /// Controller component for the air strike plane that flies over and drops a projectile.
    /// </summary>
    class AirStrikePlaneController : MonoBehaviour {
        AirStrikeStrategy _strategy;
        Vector3 _dropPosition;
        Vector3 _endPosition;
        float _speed;
        bool _hasDropped;

        public void Initialize(
            AirStrikeStrategy strategy,
            Vector3 dropPosition,
            Vector3 endPosition,
            float speed
        ) {
            _strategy = strategy;
            _dropPosition = dropPosition;
            _endPosition = endPosition;
            _speed = speed;
            _hasDropped = false;
        }

        void Update() {
            // Move plane towards end position
            var direction = (_endPosition - transform.position).normalized;
            transform.position += _speed * Time.deltaTime * direction;

            // Check if plane is over drop position (within small threshold on XZ plane)
            var horizontalDistanceToDrop = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(_dropPosition.x, 0, _dropPosition.z)
            );

            // Drop projectile when directly above target
            if (!_hasDropped && horizontalDistanceToDrop < _speed * Time.deltaTime * 2f) {
                _hasDropped = true;
                _strategy.SpawnProjectile(transform.position);
            }

            // Check if reached end position
            var distanceToEnd = Vector3.Distance(transform.position, _endPosition);
            if (distanceToEnd < _speed * Time.deltaTime * 2f) {
                Destroy(gameObject);
            }
        }
    }
}

