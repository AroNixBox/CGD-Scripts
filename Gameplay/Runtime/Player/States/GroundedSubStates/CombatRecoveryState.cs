using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    public class CombatRecoveryState : IState {
        PlayerAnimatorController _animatorController;
        PlayerCameraControls _cameraControls;
        
        public CombatRecoveryState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _animatorController = controller.AnimatorController;
        }

        public void OnEnter() {
            _cameraControls.ResetCameras();
        }
        public void Tick(float deltaTime) { }
        /// <summary>
        /// Waiting for the Attack Animation to finish before going back to the non-auth state.
        /// Because Non-Auth State will switch to T-Pose and this would cancel the Attack Animation abruptly.
        /// </summary>
        public bool IsRecoveryFinished() {
            return !_animatorController.IsInTransition(AnimationParameters.GetAnimationLayer(0)) &&
                   _animatorController.IsCurrentAnimationFinished(0.9f, 0);
        }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.cyan;
        }
    }
}