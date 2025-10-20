namespace Extensions.FSM {
    public interface ISubStateMachine : IState {
        // It is super important to Set the Current State to null when exiting the SubStateMachine
        // And reseting it to the first state when entering it again
        public IState GetCurrentState();
    }
}
