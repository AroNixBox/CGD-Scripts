using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class FallingState : IState {
        readonly PlayerController _controller;
        public FallingState(PlayerController controller) {
            _controller = controller;
        }
        public void OnEnter() { }
        public void Tick() { }
        public void OnExit() { }
        // TODO
        public Color GizmoState() {
            return Color.navyBlue;
        }
    }
}