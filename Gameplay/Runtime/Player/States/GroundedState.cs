using System;
using Core.Runtime.Service.Input;
using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class GroundedState : ISubStateMachine {
        readonly StateMachine _stateMachine;
        IState CurrentState => _stateMachine.GetCurrentState();
        
        // References
        readonly PlayerController _controller;
        readonly InputReader _inputReader;
        
        // States
        readonly IState _combatStanceState;
        readonly IState _locomotionState;
        readonly IState _awaitingAuthorityState;

        public GroundedState(PlayerController controller) {
            _inputReader = controller.InputReader;
            _controller = controller;
            
            _stateMachine = new StateMachine();
            _awaitingAuthorityState = new AwaitingAuthorityState(controller);
            _combatStanceState = new CombatStanceState(controller);
            _locomotionState = new LocomotionState(controller);

            // These two can not be Event based, since the status of authority can change outside of grounded
            At(_awaitingAuthorityState, _locomotionState, HasAuthority);
            Any(_awaitingAuthorityState, () => !HasAuthority());
            
            return;
            
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
            void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
        }
        bool HasAuthority() => _controller.AuthorityEntity.HasAuthority();
        
        public void OnEnter() {
            _stateMachine.SetState(_awaitingAuthorityState);
            
            _inputReader.Combat += OnCombat;
            _inputReader.StopCombat += OnStopCombat;
            
            _controller.OnGroundContactRegained();
        }

        void OnCombat() {
            if(!HasAuthority()) return;
            if(CurrentState is not LocomotionState) return; // Only allow transitions from Combat Stance
            
            _stateMachine.SetState(_combatStanceState);
        }

        // Switch from Combat to Locomotion when in Combat state and having Auth
        void OnStopCombat() {
            if(!HasAuthority()) return;
            if(CurrentState is not CombatStanceState) return;
            
            _stateMachine.SetState(_locomotionState);
        }
        public void Tick() {
            _stateMachine?.Tick();
        }

        public void OnExit() {
            _inputReader.Combat -= OnCombat;
            _inputReader.StopCombat -= OnStopCombat;
            
            _stateMachine.SetState(null);
        }
        public Color GizmoState() {
            return Color.grey;
        }

        public IState GetCurrentState() => _stateMachine.GetCurrentState();
    }
}