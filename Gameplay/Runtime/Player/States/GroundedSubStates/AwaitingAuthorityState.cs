using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    /// <summary>
    /// Awaiting until authority is granted.
    /// </summary>
    public class AwaitingAuthorityState : IState {
        PlayerCameraControls _cameraControls;
        public AwaitingAuthorityState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
        }

        public void OnEnter() {
            _cameraControls.ResetControllableCameras();
        }
        public void Tick(float deltaTime) { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.softYellow;
        }
    }
}