using UnityEngine;

namespace Extensions.FSM {
    public interface IState { 
        void OnEnter();
        void Tick(float deltaTime);
        void OnExit();
       
        Color GizmoState();
    } 
}