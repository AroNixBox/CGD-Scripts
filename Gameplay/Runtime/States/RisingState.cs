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
            Debug.Log("Exiting Rising State");
        }
        // TODO
        public Color GizmoState() {
            throw new System.NotImplementedException();
        }
    }
}