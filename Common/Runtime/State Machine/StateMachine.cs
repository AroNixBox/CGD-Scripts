using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions.FSM
{
    public class StateMachine {
        IState _currentState;

        readonly Dictionary<IState, List<Transition>> _transitions = new ();
        List<Transition> _currentTransitions = new ();
        readonly List<Transition> _anyTransitions = new ();

        static readonly List<Transition> EmptyTransitions = new (0);
    
        public void Tick() {
            var transition = GetTransition();
            if (transition != null)
                SetState(transition.To);
            
            _currentState?.Tick();
        }

        public void SetState(IState state) {
            if (state == _currentState)
                return;
            
            _currentState?.OnExit();
            _currentState = state;
            
            _transitions.TryGetValue(_currentState, out _currentTransitions);
            _currentTransitions ??= EmptyTransitions;
            
            _currentState.OnEnter();
        }
    
        public void AddTransition(IState from, IState to, Func<bool> predicate) {
            if (!_transitions.TryGetValue(from, out var transitions)) {
                transitions = new List<Transition>();
                _transitions[from] = transitions;
            }
            
            transitions.Add(new Transition(to, predicate));
        }
    
        public void AddAnyTransition(IState state, Func<bool> predicate) {
            _anyTransitions.Add(new Transition(state, predicate));
        }

        Transition GetTransition() {
            foreach (var transition in _anyTransitions.Where(transition => transition.Condition()))
                return transition;

            return _currentTransitions.FirstOrDefault(transition => transition.Condition());
        }

        class Transition {
            public Func<bool> Condition { get; }
            public IState To { get; }
    
            public Transition(IState to, Func<bool> condition) {
                To = to;
                Condition = condition;
            }
        }

        /// <returns>The Current State of this State Machine.
        /// If a State is a SubStateMachine, it needs to be manually asked
        /// For its CurrentSubState (GetCurrentState() is ISubStateMachine subStateMachine ? subStateMachine.GetCurrentState() : LACHS)</returns>
        public IState GetCurrentState() => _currentState; 
        public Color GetGizmoColor()
        {
            return _currentState?.GizmoState() ?? Color.black;
        }
        public string GetCurrentStateName()
        {
            return _currentState?.GetType().Name ?? "No State";
        }
    }
}
