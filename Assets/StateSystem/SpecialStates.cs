using UnityEngine;
public class SpecialStates : State {
    public State sideB;
    public override void Enter() {
        base.Enter();
        Set(sideB, true);

    }
    public override void Do() {
        base.Do();
        if (state.complete) {
            Complete("finished");
        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
    }
}

