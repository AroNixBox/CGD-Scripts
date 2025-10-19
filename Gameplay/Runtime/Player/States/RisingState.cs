using Extensions.FSM;
using UnityEngine;

namespace Gameplay.Runtime {
    public class RisingState : IState {
        readonly PlayerController _controller;
        public RisingState(PlayerController controller) {
            _controller = controller;
        }
        public void OnEnter() {
            _controller.OnGroundContactLost();
        }
        public void Tick() {

        }
        public void OnExit() {
        }
        // TODO
        public Color GizmoState() {
            return Color.skyBlue;
        }
    }
}