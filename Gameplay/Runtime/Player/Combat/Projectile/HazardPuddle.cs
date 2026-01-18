using System.Collections.Generic;
using UnityEngine;

public class HazardPuddle : MonoBehaviour {
    [Header("Puddle Settings")]
    [SerializeField] private GameObject droplet;
    [SerializeField] private float puddleRadius = 1.2f;
    [SerializeField] private float minPointDistance = 0.15f;
    [SerializeField] private int poissonTries = 30;

    [Header("Raycast Settings")]
    [SerializeField] private float raycastHeight = 3.0f;
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private LayerMask surfaceMask;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float gizmoSphereRadius = 0.05f;

    private readonly List<Vector3> sampledPoints = new();

    public void GeneratePuddle(Vector3 hitPoint, Vector3 hitNormal) {
        sampledPoints.Clear();
        BuildBasis(hitNormal, out Vector3 right, out Vector3 forward);
        List<Vector2> poissonPoints = GeneratePoissonDisk2D(puddleRadius, minPointDistance, poissonTries);

        foreach (Vector2 p in poissonPoints) {
            Vector3 worldFlat =
                hitPoint +
                right * p.x +
                forward * p.y;

            Vector3 origin = worldFlat + hitNormal * raycastHeight;

            if (Physics.Raycast(
                origin,
                -hitNormal,
                out RaycastHit hit,
                raycastDistance,
                surfaceMask)) {
                Vector3 finalPos = hit.point;
                sampledPoints.Add(finalPos);

                if (!drawGizmos && droplet != null) {
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    GameObject d = Instantiate(droplet, finalPos, rot, transform);
                    d.transform.localScale *= Random.Range(0.9f, 1.1f);

                }
            }
        }
    }

    private void BuildBasis(Vector3 normal, out Vector3 right, out Vector3 forward) {
        right = Vector3.Cross(normal, Vector3.up);

        if (right.sqrMagnitude < 0.001f) {
            right = Vector3.Cross(normal, Vector3.forward);
        }

        right.Normalize();
        forward = Vector3.Cross(right, normal).normalized;
    }

    private List<Vector2> GeneratePoissonDisk2D(float radius, float minDist, int tries) {
        List<Vector2> points = new();
        float radiusSq = radius * radius;

        for (int i = 0; i < tries * 20; i++) {
            Vector2 p = Random.insideUnitCircle * radius;

            if (p.sqrMagnitude > radiusSq) {
                continue;
            }

            bool valid = true;
            foreach (Vector2 q in points) {
                if ((p - q).sqrMagnitude < minDist * minDist) {
                    valid = false;
                    break;
                }
            }

            if (valid) {
                points.Add(p);
            }
        }

        return points;
    }

    private void OnDrawGizmos() {
        if (!drawGizmos || sampledPoints == null) {
            return;
        }

        Gizmos.color = Color.blue;

        foreach (Vector3 p in sampledPoints) {
            Gizmos.DrawSphere(p, gizmoSphereRadius);
        }
    }
}
