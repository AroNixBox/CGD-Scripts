using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime.Player.States.GroundedSubStates {
    public class EmptyState : IState {
        public void OnEnter() { }
        public void Tick(float deltaTime) { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.cyan;
        }
    }
}