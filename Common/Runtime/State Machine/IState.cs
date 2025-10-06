using UnityEngine;

namespace Extensions.FSM
{
    public interface IState
    { 
        void OnEnter();
        void Tick();
        void OnExit();
       
        Color GizmoState();
    } 
}