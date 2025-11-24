using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.Runtime.Cinematics {
    public class DollyCameraController : MonoBehaviour {
        [Tooltip("Negate Speed if you want to move backwards on the curve (From Last to First knot)")]
        [SerializeField] int speed;
        [SerializeField, Required] CinemachineCamera cinemachineCamera;
        [SerializeField, Required] Transform cinemachineSplineEndPoint;
        [SerializeField, Required] CinemachineSplineDolly cinemachineSplineDolly;
        
        const float MinimumDistance = 0.5f;
        const int CameraInactivePriority = 0;

        public async UniTask MoveDollyToTarget(CancellationToken token) {
            while (!token.IsCancellationRequested &&
                   Vector3.Distance(cinemachineCamera.transform.position, cinemachineSplineEndPoint.position) > MinimumDistance) {
                cinemachineSplineDolly.CameraPosition += speed * Time.fixedDeltaTime;
                await UniTask.WaitForFixedUpdate(token);
            }

            if (token.IsCancellationRequested) return;

            cinemachineCamera.Priority = CameraInactivePriority;
        }
    }
}
