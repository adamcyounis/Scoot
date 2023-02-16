using UnityEngine;

public class StateMachine : MonoBehaviour {
    [HideInInspector]
    public State state;
    public void Set(State newState, bool overRide = false) {

        if (state != newState || overRide) {
            if (state != null) {
                state.Exit();
            }
            state = newState;
            state.Setup(this);
            state.Enter();
        }
    }
}
