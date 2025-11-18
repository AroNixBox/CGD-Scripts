using System;
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
        public event Action OnDollyCameraTargetReached = delegate { };

        const float MinimumDistance = 0.5f;
        const int CameraInactivePriority = 0;
        bool _triggered;
        bool _allowDollyStart;

        public void InitDolly() {
            _allowDollyStart = true;
        }
        
        void FixedUpdate() {
            if (!_allowDollyStart) return;
            if (_triggered) return;
            
            cinemachineSplineDolly.CameraPosition += speed * Time.fixedDeltaTime;
            if (Vector3.Distance(cinemachineCamera.transform.position, cinemachineSplineEndPoint.position) >
                MinimumDistance) return;
            
            _triggered = true;
            cinemachineCamera.Priority = CameraInactivePriority;
            OnDollyCameraTargetReached?.Invoke();
        }
    }
}
