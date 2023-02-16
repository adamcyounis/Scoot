using UnityEngine;
using System;
public class Jump : State {
    public bool inputtingJump => Input.GetButton("Jump");
    public float fullJumpTime = 0.2f;
    public float releaseDecay = 0.75f;

    public float shortHopTime = 0.1f;
    public float jumpSpeed;
    bool released;
    public override void Enter() {
        released = false;
        core.velY = jumpSpeed;
        base.Enter();
    }
    public override void Do() {
        if (time > shortHopTime && time < fullJumpTime) {
            if (inputtingJump && !released) {
                core.velY = jumpSpeed;
            } else {
                released = true;
                core.velY *= releaseDecay;

            }
        } else {
            Complete("jump finished");

        }

        base.Do();
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
    }

}
