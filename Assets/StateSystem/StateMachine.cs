using UnityEngine;

public class StateMachine : MonoBehaviour {
    public State state;
    public void Set(State newState, bool overRide = false) {

        if (state != newState || overRide) {
            state.Exit();
            state = newState;
            state.Setup(this);
            state.Enter();
        }
    }
}
