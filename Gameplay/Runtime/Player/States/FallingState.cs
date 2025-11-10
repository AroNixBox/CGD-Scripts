using Common.Runtime;
using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using UnityEngine;

namespace Gameplay.Runtime.Player.States {
    public class FallingState : IState {
        readonly PlayerController _controller;
        readonly PlayerAnimatorController _animatorController;
        
        readonly StopwatchTimer _timer;
        public FallingState(PlayerController controller) {
            _controller = controller;
            _animatorController = controller.AnimatorController;
            _timer = new StopwatchTimer();
        }

        public void OnEnter() {
            _controller.OnGroundContactLost();
            _animatorController.ChangeAnimationState(AnimationParameters.Fall);
            _timer.Start();
        }

        public void Tick(float deltaTime) {
            _timer.Tick(deltaTime);
        }
        
        public float GetFallingTime() => _timer.GetTime();

        public void OnExit() {
            _timer.Reset();
        }
        public Color GizmoState() {
            return Color.navyBlue;
        }
    }
}