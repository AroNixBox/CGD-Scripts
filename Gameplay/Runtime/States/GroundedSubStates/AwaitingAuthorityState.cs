using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    /// <summary>
    /// Noop, just awaiting until authority is granted.
    /// </summary>
    public class AwaitingAuthorityState : IState {
        public void OnEnter() { }
        public void Tick() { }
        public void OnExit() { }
        public Color GizmoState() {
            return Color.darkRed;
        }
    }
}