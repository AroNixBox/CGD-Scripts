using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour {
        public static TrajectoryPredictor Instance;
        
        [SerializeField] int resolution;
        [SerializeField, Range(0.01f, 0.5f), Tooltip("The time increment used to calculate the trajectory")]
        float increment = 0.025f;
        [SerializeField, Range(1.05f, 2f), Tooltip("The raycast overlap between points in the trajectory, this is a multiplier of the length between points. 2 = twice as long")]
        float rayOverlap = 1.1f;

        LineRenderer _lineRenderer;

        void Awake() {
            if (Instance != null)
                Destroy(this);
            else
                Instance = this;
            
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void PredictTrajectory(WeaponProperties weaponProperties, float projectileForce, float projectileMass, float projectileDrag) {
            var velocity = projectileForce / projectileMass *
                           weaponProperties.ShootDirection;
            var position = weaponProperties.MuzzlePosition;
            
            _lineRenderer.positionCount = resolution;

            for (var i = 0; i < resolution; i++) {
                velocity = CalculateNewVelocity(velocity, projectileDrag);
                var nextPosition = position + velocity * increment;
                var overlap = Vector3.Distance(position, nextPosition) * rayOverlap;
                _lineRenderer.SetPosition(i, position);
    
                if (Physics.Raycast(position, velocity.normalized, out RaycastHit hit, overlap)) {
                    if (i + 1 < resolution) {
                        _lineRenderer.SetPosition(i + 1, hit.point);
                        _lineRenderer.positionCount = i + 2;
                    } else { // last point
                        _lineRenderer.SetPosition(i, hit.point);
                        _lineRenderer.positionCount = i + 1;
                    }
                    break;
                }
                
                position = nextPosition;
            }
        }

        Vector3 CalculateNewVelocity(Vector3 velocity, float drag) {
            velocity += Physics.gravity * increment;
            velocity *= Mathf.Clamp01(1f - drag * increment);
            return velocity;
        }

        public void RemoveTrajectoryLine() {
            _lineRenderer.positionCount = 0;
        }
    }
}