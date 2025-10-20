using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using UnityEngine;

namespace Gameplay.Runtime {
    public class FallingState : IState {
        readonly PlayerController _controller;
        readonly PlayerAnimatorController _animatorController;
        public FallingState(PlayerController controller) {
            _controller = controller;
            _animatorController = controller.AnimatorController;
        }

        public void OnEnter() {
            _animatorController.ChangeAnimationState(AnimationParameters.Fall);
        }
        public void Tick() { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.navyBlue;
        }
    }
}