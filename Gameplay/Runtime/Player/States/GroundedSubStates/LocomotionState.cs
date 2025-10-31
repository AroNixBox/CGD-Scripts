using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    public class LocomotionState : IState {
        readonly PlayerCameraControls _cameraControls;
        readonly PlayerAnimatorController _animatorController;
        readonly PlayerController _playerController;
        public LocomotionState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _animatorController = controller.AnimatorController;
            _playerController = controller;
        }

        public void OnEnter() {
            _cameraControls.SwitchToCameraMode(PlayerCameraControls.CameraMode.ThirdPerson);
            _animatorController.ChangeAnimationState(AnimationParameters.Locomotion);
        }

        public void Tick(float deltaTime) {
            var currentMoveSpeed = _playerController.GetMovementVelocity();
            _animatorController.UpdateAnimatorSpeed(currentMoveSpeed.magnitude);
        }

        public void OnExit() {
            _animatorController.UpdateAnimatorSpeed(0);        
        }
        public Color GizmoState() {
            return Color.darkGreen;
        }
    }
}