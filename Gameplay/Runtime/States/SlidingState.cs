using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class SlidingState : IState {
        readonly PlayerController _controller;
        
        public SlidingState(PlayerController controller) {
            _controller = controller;
        }
        
        public void OnEnter() {
            Debug.Log("Entering Sliding State");
        }
        public void Tick() { }

        public void OnExit() {
            Debug.Log("Exiting Sliding State");
        }
        // TODO
        public Color GizmoState() {
            throw new System.NotImplementedException();
        }
    }
}