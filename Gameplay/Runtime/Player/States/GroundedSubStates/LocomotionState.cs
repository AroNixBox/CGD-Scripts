using Extensions.FSM;
using Gameplay.Runtime.Player;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
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
            _ = _cameraControls.SwitchToControllableCameraMode(PlayerCameraControls.ECameraMode.ThirdPerson);
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