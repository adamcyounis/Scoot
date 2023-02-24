using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rest : State {

    public AnimationState restAttack;
    public AnimationState restLoop;
    public override void Enter() {
        base.Enter();
        Set(restAttack, true);
        core.canMove = false;
        body.velocity = body.velocity.normalized * 0.2f;
        body.gravityScale = 0.5f;
    }

    public override void Do() {
        base.Do();
        if (state.complete) {
            if (state == restAttack) {
                Set(restLoop);
            } else {
                Complete("finished");
                core.canMove = true;
            }

        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
        core.canMove = true;
        core.ReturnToDefaultBodyProps();
    }
}
