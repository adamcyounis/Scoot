using UnityEngine;
public class SpecialStates : State {
    public State sideB;
    public Rest rest;
    public override void Enter() {
        base.Enter();
        core.canAttack = false;
        if (core.input.movement.y < -0.5f) {
            Set(rest, true);

        } else {
            Set(sideB, true);

        }

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
        core.canAttack = true;
    }
}

