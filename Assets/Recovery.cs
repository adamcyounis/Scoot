using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recovery : State {
    public AnimationState startup;
    public FireFoxActive end;
    bool canRecover;
    public Jump jump;
    private void Start() {
        core.selfAwareness.gotGrounded.AddListener(RespondToGrounded);
    }
    public override void Enter() {
        base.Enter();
        canRecover = false;
        Set(startup, true);
        core.canAttack = false;
        core.canMove = false;
        core.body.gravityScale = 0;
        core.body.velocity = Vector2.zero;
        if (jump != null) {
            jump.remainingJumps = 0;
        }

    }

    public override void Do() {
        base.Do();
        if (state.complete) {
            if (state == startup) {
                Set(end);
            } else {
                Complete("finished!");
            }
        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
        core.ReturnToDefaultBodyProps();
    }

    public bool ShouldRecover() {
        return canRecover;
    }
    public void RespondToGrounded() {
        canRecover = true;
    }
}
