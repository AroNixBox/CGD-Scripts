using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    /// <summary>
    /// Awaiting until authority is granted.
    /// </summary>
    public class AwaitingAuthorityState : IState {
        readonly PlayerCameraControls _cameraControls;
        readonly PlayerController _controller;
        
        public AwaitingAuthorityState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _controller = controller;
        }

        public void OnEnter() {
            _cameraControls.ResetControllableCameras();
        }
        public void Tick(float deltaTime) { }
        public void OnExit() {
            _controller.MovementBudget.Reset();
        }
        public Color GizmoState() {
            return Color.softYellow;
        }
    }
}