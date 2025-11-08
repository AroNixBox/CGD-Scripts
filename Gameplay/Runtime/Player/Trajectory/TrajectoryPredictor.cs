using System;
using UnityEngine;

namespace Gameplay.Runtime.Player.Trajectory {
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour {
        [SerializeField] int resolution;
        [SerializeField, Range(0.01f, 0.5f), Tooltip("The time increment used to calculate the trajectory")]
        float increment = 0.025f;
        [SerializeField, Range(1.05f, 2f), Tooltip("The raycast overlap between points in the trajectory, this is a multiplier of the length between points. 2 = twice as long")]
        float rayOverlap = 1.1f;

        LineRenderer _lineRenderer;

        void Awake() {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void PredictTrajectory(ProjectileProperties projectileProperties) {
            var velocity = projectileProperties.initialSpeed / projectileProperties.mass *
                           projectileProperties.direction;
            var position = projectileProperties.initialPosition;
            
            _lineRenderer.positionCount = resolution;


            for (var i = 0; i < resolution; i++) {
                velocity = CalculateNewVelocity(velocity, projectileProperties.drag);
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

        void UpdateLineRenderer(int count, (int point, Vector3 pos) pointPos) {
            _lineRenderer.positionCount = count;
            _lineRenderer.SetPosition(pointPos.point, pointPos.pos);
        }
    }
}