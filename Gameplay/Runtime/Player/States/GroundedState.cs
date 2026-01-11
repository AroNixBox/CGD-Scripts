using System;
using Core.Runtime.Service.Input;
using Extensions.FSM;
using Gameplay.Runtime.Player.Combat;
using Gameplay.Runtime.Player.States.GroundedSubStates;
using UnityEngine;

namespace Gameplay.Runtime.Player.States {
    public class GroundedState : ISubStateMachine {
        readonly StateMachine _stateMachine;
        IState CurrentState => _stateMachine.GetCurrentState();
        
        // References
        readonly PlayerController _controller;
        readonly InputReader _inputReader;
        
        // States
        readonly IState _combatStanceState;
        readonly IState _locomotionState;

        // Shared Transition Data
        Projectile _pendingProjectile;

        public GroundedState(PlayerController controller) {
            _inputReader = controller.InputReader;
            _controller = controller;
            
            _stateMachine = new StateMachine();
            IState awaitingAuthorityState = new AwaitingAuthorityState(controller);
            var projectileWatchState = new ProjectileWatchState(controller, () => _pendingProjectile);
            _combatStanceState = new CombatStanceState(controller, projectile => _pendingProjectile = projectile);
            _locomotionState = new LocomotionState(controller);

            // These two can not be Event based, since the status of authority can change outside of grounded
            At(_combatStanceState, projectileWatchState, () => _pendingProjectile != null);
            At(projectileWatchState, awaitingAuthorityState, () => projectileWatchState.IsComplete);
            At(awaitingAuthorityState, _locomotionState, HasAuthority);
            At(_locomotionState, awaitingAuthorityState, () => !HasAuthority());
            
            return;
            
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
        }
        
        bool HasAuthority() => _controller.AuthorityEntity.HasAuthority();
        
        public void OnEnter() {
            // Set to locomotion state to force the grounded animation
            _stateMachine.SetState(_locomotionState);
            
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
        public void Tick(float deltaTime) {
            _stateMachine?.Tick(deltaTime);
            
            // Reset pending projectile after transition
            if (_pendingProjectile != null && CurrentState is ProjectileWatchState) {
                _pendingProjectile = null;
            }
        }

        public void OnExit() {
            _inputReader.Combat -= OnCombat;
            _inputReader.StopCombat -= OnStopCombat;
            
            _stateMachine.ResetState();
        }
        public Color GizmoState() {
            return Color.grey;
        }

        public IState GetCurrentState() => _stateMachine.GetCurrentState();
    }
}