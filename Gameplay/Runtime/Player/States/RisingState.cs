using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using UnityEngine;

namespace Gameplay.Runtime {
    public class RisingState : IState {
        readonly PlayerAnimatorController _animatorController;
        readonly PlayerController _controller;
        public RisingState(PlayerController controller) {
            _controller = controller;
            _animatorController = controller.AnimatorController;
        }
        public void OnEnter() {
            _controller.OnGroundContactLost();
            _animatorController.ChangeAnimationState(AnimationParameters.Fall);
        }
        public void Tick() {

        }
        public void OnExit() {
        }
        public Color GizmoState() {
            return Color.skyBlue;
        }
    }
}