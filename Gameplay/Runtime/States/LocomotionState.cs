using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class LocomotionState : IState {
        public void OnEnter() { }
        public void Tick() { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.darkGreen;
        }
    }
}