using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    // TODO: Make SubStateMachine
    // Handle GetCurrentState to also be able to return the SubStateMachine itsself
    // Currently we only return the current state of the SubStateMachine
    public class GroundedState : IState {
        
        readonly PlayerController _controller;
        public GroundedState(PlayerController controller) {
            _controller = controller;
        }
        public void OnEnter() {
            Debug.Log("Entering GroundedState");
        }
        public void Tick() { }
        public void OnExit() {
            Debug.Log("Exiting GroundedState");
        }
        public Color GizmoState() {
            throw new System.NotImplementedException();
        }
    }
}