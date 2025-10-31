using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using Gameplay.Runtime.Player.Camera;
using UnityEngine;

namespace Gameplay.Runtime {
    /// <summary>
    /// Awaiting until authority is granted.
    /// </summary>
    public class AwaitingAuthorityState : IState {
        readonly PlayerAnimatorController _animatorController;

        public AwaitingAuthorityState(PlayerController controller) {
            _animatorController = controller.AnimatorController;
        }

        public void OnEnter() {
            _animatorController.ChangeAnimationState(AnimationParameters.TPose);
        }
        public void Tick(float deltaTime) { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.softYellow;
        }
    }
}