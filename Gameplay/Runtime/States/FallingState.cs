using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class FallingState : IState {
        readonly PlayerController _controller;
        public FallingState(PlayerController controller) {
            _controller = controller;
        }
        public void OnEnter() {
            Debug.Log("Entering FallingState");
        }
        public void Tick() { }
        public void OnExit() {
            Debug.Log("Exiting FallingState");
        }
        // TODO
        public Color GizmoState() {
            return Color.navyBlue;
        }
    }
}