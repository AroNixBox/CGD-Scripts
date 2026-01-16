using Extensions.FSM;
using Gameplay.Runtime.Player.Animation;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    // TODO: Usable for player death instead of getting destroyed!
    public class AnimationState : IState {
        readonly PlayerAnimatorController _animatorController;
        readonly bool _enableRootMotion;
        readonly int _stateHashName;

        public AnimationState(PlayerController controller, int stateHashName, bool enableRootMotion) {
            _animatorController = controller.AnimatorController;
            _stateHashName = stateHashName;
            _animatorController.SetRootMotion(enableRootMotion);
        }
        public void OnEnter() {
            _animatorController.ChangeAnimationState(_stateHashName);
        }
        public void Tick(float deltaTime) { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.salmon;
        }
    }
}