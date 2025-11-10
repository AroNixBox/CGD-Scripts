using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Camera;
using Gameplay.Runtime.Player;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using Unity.Cinemachine;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    public class CombatRecoveryState : IState {
        // PlayerAnimatorController _animatorController;
        readonly PlayerCameraControls _cameraControls;
        readonly PlayerController _controller;
        public CombatRecoveryState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _controller = controller;
            //_animatorController = controller.AnimatorController;
        }

        public void OnEnter() {
            // TODO: Trigger Bullet Cam
            _cameraControls.ResetControllableCameras(); // Arena Cam, because highes Priority
            _ = HardCodedExitTimeBuffer();
        }
        public void Tick(float deltaTime) { }

        bool _recoveryFinished;
        /// <summary>
        /// Waiting for the Attack Animation to finish before going back to the non-auth state.
        /// Because Non-Auth State will switch to T-Pose and this would cancel the Attack Animation abruptly.
        /// </summary>
        public bool IsRecoveryFinished() {
            // TODO:
            // If we would want to wait for an Animation:
            // return !_animatorController.IsInTransition(AnimationParameters.GetAnimationLayer(0)) &
            //              _animatorController.IsCurrentAnimationFinished(0.9f, 0);
            
            return _recoveryFinished;
        }

        async UniTask HardCodedExitTimeBuffer() {
            await UniTask.WaitForSeconds(3);
            _recoveryFinished = true;
        }

        public void OnExit() {
            _cameraControls.ResetBulletCamera();
            
            // Exit Condition
            _recoveryFinished = false;
            
            // Entry Condition for next Player Substatemachine
            _controller.AuthorityEntity.GiveNextAuthority();
        }
        public Color GizmoState() {
            return Color.cyan;
        }
    }
}