using Extensions.FSM;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    public class CombatStanceState : IState {
        readonly PlayerCameraControls _cameraControls;
        public CombatStanceState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
        }

        public void OnEnter() {
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.FirstPerson);
        }
        
        public void Tick() { }

        public void OnExit() { }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}