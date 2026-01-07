using System.Threading;
using Core.Runtime.Service;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace Core.Runtime.Cinematics {
    public class DollyCameraController : MonoBehaviour {
        [Tooltip("Negate Speed if you want to move backwards on the curve (From Last to First knot)")]
        [SerializeField] float speed;
        [SerializeField, Required] CinemachineCamera cinemachineCamera;
        [SerializeField, Required] Transform cinemachineSplineEndPoint;
        [SerializeField, Required] CinemachineSplineDolly cinemachineSplineDolly;

        [SerializeField] bool performDolly;
        
        const float MinimumDistance = 0.5f;
        const int CameraActivePriority = 20;
        const int CameraInactivePriority = 0;

        async void OnEnable() {
            if (!performDolly) return;
            if (!ServiceLocator.TryGet(out GameManager gameManager)) {
                await UniTask.WaitUntil(() => ServiceLocator.TryGet(out gameManager));
                if (gameManager == null) return;
            }
            gameManager.OnPreGameInit += HandlePreGameInit;
        }

        void OnDisable() {
            if (!performDolly) return;
            if (!ServiceLocator.TryGet(out GameManager gameManager)) return;
            gameManager.OnPreGameInit -= HandlePreGameInit;
        }

        /// <summary>
        /// Registers the dolly movement as an async initialization task.
        /// The GameManager will await this task before starting the game.
        /// </summary>
        void HandlePreGameInit(object sender, GameManager.PreGameInitEventArgs args) {
            // Lifetime
            var token = this.GetCancellationTokenOnDestroy();

            // Dolly w/ pre game init task
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
