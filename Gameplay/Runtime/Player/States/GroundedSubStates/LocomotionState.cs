using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    public class LocomotionState : IState {
        readonly PlayerCameraControls _cameraControls;
        public LocomotionState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
        }

        public void OnEnter() {
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.ThirdPerson);
        }
        public void Tick() { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.darkGreen;
        }
    }
}