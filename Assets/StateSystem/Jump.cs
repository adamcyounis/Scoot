using UnityEngine;
using System;
public class Jump : State, AirState {
    public bool holdingJump => core.input.jumpHeld;

    public bool inputtingJump => core.input.jumpPressed;
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
        if (!core.selfAwareness.grounded) {
            remainingJumps--;
        }

        released = false;
        core.velY = jumpSpeed;
        base.Enter();
        remainingJumps--;

        float h = core.input.movement.x;
        if (Mathf.Abs(h) > 0.1f) {
            core.velX = Mathf.Sign(h);
        }
    }

    public override void Do() {
        if (time > shortHopTime && time < fullJumpTime) {
            if (holdingJump && !released) {
                core.velY = jumpSpeed;
            } else {
                released = true;
                core.velY *= releaseDecay;

            }
        }

        if (time > fullJumpTime || released) {
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
