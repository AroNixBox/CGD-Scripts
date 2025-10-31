using Cysharp.Threading.Tasks;
using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    /// <summary>
    /// Awaiting until authority is granted.
    /// </summary>
    public class AwaitingAuthorityState : IState {
        readonly PlayerCameraControls _cameraControls;
        readonly PlayerAnimatorController _animatorController;

        public AwaitingAuthorityState(PlayerController controller) {
            _cameraControls = controller.PlayerCameraControls;
            _animatorController = controller.AnimatorController;
        }

        public void OnEnter() {
            _cameraControls.ResetCameras();
            _ = WaitSomeDelayThenNormalAnim();
        }

        // TODO:
        // Currently after Attacking we switch right into AwaitingAuthorityState, this causes a clash in the animation that is to be played
        // And the call to ChangeAnimationState in OnEnter of this state. Because both calls happen so fast one after another, the first call wins..
        // This is why we have this hardcoded waitfor...
        // Solution, InBetween-State after player has attacked that waits until the attack animation is fully done playing
        async UniTask WaitSomeDelayThenNormalAnim() {
            await UniTask.Delay(1000);
            _animatorController.ChangeAnimationState(AnimationParameters.TPose);
        }
        public void Tick(float deltaTime) { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.softYellow;
        }
    }
}