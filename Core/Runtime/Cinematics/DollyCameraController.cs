using System.Threading;
using Core.Runtime.Service;
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

        [SerializeField] bool performDolly;
        
        const float MinimumDistance = 0.5f;
        const int CameraActivePriority = 20;
        const int CameraInactivePriority = 0;

        void OnEnable() {
            if (!performDolly) return;
            if (!ServiceLocator.TryGet(out GameManager gameManager)) return;
            gameManager.OnGameInit += HandleGameInit;
        }

        void OnDisable() {
            if (!performDolly) return;
            if (!ServiceLocator.TryGet(out GameManager gameManager)) return;
            gameManager.OnGameInit -= HandleGameInit;
        }

        /// <summary>
        /// Registers the dolly movement as an async initialization task.
        /// The GameManager will await this task before starting the game.
        /// </summary>
        void HandleGameInit(object sender, GameManager.GameInitEventArgs args) {
            // Use this object's lifetime as cancellation source.
            var token = this.GetCancellationTokenOnDestroy();

            // Register camera movement as part of the init sequence.
            args.CompletionTasks.Add(MoveDollyToTarget(token));
        }

        async UniTask MoveDollyToTarget(CancellationToken token) {
            cinemachineCamera.Priority = CameraActivePriority;
            
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
