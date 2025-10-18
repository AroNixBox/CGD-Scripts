using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class CombatStanceState : IState {

        public CombatStanceState(PlayerController controller) { }

        public void OnEnter() { }
        
        public void Tick() { }

        public void OnExit() { }
        public Color GizmoState() {
            return Color.darkOrange;
        }
    }
}