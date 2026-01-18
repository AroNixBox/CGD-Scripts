using Core.Runtime.Service;
using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour {
        [SerializeField, Tooltip("Maximum distance the trajectory should predict")]
        float range = 10f;
        [SerializeField, Tooltip("Distance between each point on the trajectory line")]
        float pointSpacing = 0.5f;

        LineRenderer _lineRenderer;

        void Awake() {
            ServiceLocator.Register(this);
            _lineRenderer = GetComponent<LineRenderer>();
        }

        void OnDestroy() => ServiceLocator.Unregister<TrajectoryPredictor>();

        public void PredictTrajectory(WeaponProperties weaponProperties, float projectileForce, float projectileMass, float projectileDrag) {
            var velocity = projectileForce / projectileMass * weaponProperties.ShootDirection;
            var position = weaponProperties.MuzzlePosition;
            
            var positions = new System.Collections.Generic.List<Vector3>();
            var traveledDistance = 0f;
            const float timeStep = 0.001f; // Small time step for accurate physics simulation
            
            positions.Add(position);
            var lastRecordedPosition = position;
            
            // Simulate trajectory until we hit something or exceed range
            while (traveledDistance < range) {
                // Update velocity and position using physics
                velocity = CalculateNewVelocity(velocity, projectileDrag, timeStep);
                var nextPosition = position + velocity * timeStep;
                
                // Check for collision
                var direction = (nextPosition - position).normalized;
                var distance = Vector3.Distance(position, nextPosition);
                
                if (Physics.Raycast(position, direction, out RaycastHit hit, distance)) {
                    positions.Add(hit.point);
                    break;
                }
                
                position = nextPosition;
                
                // Record position when we've traveled enough distance
                var distanceFromLast = Vector3.Distance(lastRecordedPosition, position);
                if (distanceFromLast >= pointSpacing) {
                    positions.Add(position);
                    lastRecordedPosition = position;
                    traveledDistance += distanceFromLast;
                }
            }
            
            // Apply positions to line renderer
            _lineRenderer.positionCount = positions.Count;
            _lineRenderer.SetPositions(positions.ToArray());
        }

        Vector3 CalculateNewVelocity(Vector3 velocity, float drag, float timeStep) {
            velocity += Physics.gravity * timeStep;
            velocity *= Mathf.Clamp01(1f - drag * timeStep);
            return velocity;
        }

        public void RemoveTrajectoryLine() {
            _lineRenderer.positionCount = 0;
        }
    }
}