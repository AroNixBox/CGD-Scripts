namespace Extensions.FSM {
    public interface ISubStateMachine : IState {
        public IState GetCurrentState();
    }
}
