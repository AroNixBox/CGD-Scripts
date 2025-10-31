using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using UnityEngine;

namespace Gameplay.Runtime {
    public class LandingState : IState {
        readonly PlayerAnimatorController _animatorController;
        
        public LandingState(PlayerController controller) {
            _animatorController = controller.AnimatorController;
        }
        public void OnEnter() {
            _animatorController.ChangeAnimationState(AnimationParameters.Land);
        }
        public void Tick(float deltaTime) { }

        public bool HasLanded() {
            var landStateHash = AnimationParameters.Land;
            return !_animatorController.IsInTransition(AnimationParameters.GetAnimationLayer(landStateHash)) &&
                   _animatorController.IsAnimationFinished(landStateHash, .5f);
        }
        public void OnExit() {
            
        }
        public Color GizmoState() {
            return Color.magenta;
        }
    }
}