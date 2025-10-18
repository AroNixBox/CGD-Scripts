using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    /// <summary>
    /// Awaiting until authority is granted.
    /// </summary>
    public class AwaitingAuthorityState : IState {
        readonly PlayerCameraControls _cameraControls;
        public AwaitingAuthorityState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
        }

        public void OnEnter() => _cameraControls.ResetCameras();
        public void Tick() { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.darkRed;
        }
    }
}