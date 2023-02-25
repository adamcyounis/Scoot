using System;
using UnityEngine;


public class FireFoxActive : State {
    public Retro.Sheet sheet;
    public AnimationCurve curve;
    Vector2 direction;
    int fixedFrames;
    public override void Enter() {
        base.Enter();
        animator.Play(sheet, 1, true, true);
        direction = core.input.movement.normalized;
        if (direction == Vector2.zero) {
            direction = Vector2.up;
        }
        body.gravityScale = 0;
        fixedFrames = 0;
        core.transform.up = direction;
    }
    public override void Do() {
        base.Do();
    }

    public override void FixedDo() {
        base.FixedDo();
        float t = fixedFrames * Time.fixedDeltaTime;
        if (t < curve.keys[curve.keys.Length - 1].time) {
            body.velocity = direction * curve.Evaluate(t);
            fixedFrames++;
        } else {
            core.transform.up = Vector3.up;

            Complete("finished animation");
        }

    }
    public override void Exit() {
        base.Exit();
        core.ReturnToDefaultBodyProps();
        core.transform.up = Vector3.up;
    }
}