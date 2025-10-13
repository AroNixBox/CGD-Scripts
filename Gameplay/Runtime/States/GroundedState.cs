using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    // TODO: Make SubStateMachine
    // Handle GetCurrentState to also be able to return the SubStateMachine itsself
    // Currently we only return the current state of the SubStateMachine
    public class GroundedState : ISubStateMachine {
        StateMachine _stateMachine;
        readonly PlayerController _controller;
        public GroundedState(PlayerController controller) {
            _controller = controller;
            
            _stateMachine = new StateMachine();
            var testState = new TestState();
            
            _stateMachine.SetState(testState);
        }
        public void OnEnter() {
            _controller.OnGroundContactRegained();
        }
        public void Tick() { }
        public void OnExit() {
            Debug.Log("Exiting GroundedState");
        }
        public Color GizmoState() {
            return Color.darkSlateGray;
        }

        public IState GetCurrentState() => _stateMachine.GetCurrentState();

        // TODO: Substatemachine for:
        // GroundLocomotion
        // Shooting Mode
    }
}