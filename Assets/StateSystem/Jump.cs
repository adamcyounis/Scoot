using UnityEngine;
using System;
public class Jump : State {
    public bool holdingJump => Input.GetButton("Jump");

    public bool inputtingJump => Input.GetButtonDown("Jump");
    public float fullJumpTime = 0.2f;
    public float releaseDecay = 0.75f;

    public float shortHopTime = 0.1f;
    public float jumpSpeed;
    public int remainingJumps = 2;

    bool released;

    void Start() {
        core.selfAwareness.gotGrounded.AddListener(GetGrounded);

    }

    void GetGrounded() {
        remainingJumps = 2;
    }

    public override void Enter() {
        released = false;
        core.velY = jumpSpeed;
        base.Enter();
        remainingJumps--;
    }

    public override void Do() {
        if (time > shortHopTime && time < fullJumpTime) {
            if (holdingJump && !released) {
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
    public bool ShouldJump() {
        return (inputtingJump && remainingJumps > 0);
    }


    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
    }

}
