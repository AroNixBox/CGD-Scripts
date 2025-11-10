using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using UnityEngine;

namespace Gameplay.Runtime.Player.States {
    public class SlidingState : IState {
        readonly PlayerController _controller;
        readonly PlayerAnimatorController _animatorController;
        
        public SlidingState(PlayerController controller) {
            _controller = controller;
            _animatorController = controller.AnimatorController;
        }
        public void OnEnter() {
            _controller.OnGroundContactLost();
            _animatorController.ChangeAnimationState(AnimationParameters.Fall);
        }
        public void Tick(float deltaTime) { }
        public void OnExit() { }
        // TODO
        public Color GizmoState() {
            return Color.softYellow;
        }
    }
}