using System;
using UnityEngine;

public class AirControl : State, AirState {
    public Jump jump;
    public State fall;

    public Retro.Sheet jumpFall;
    [Range(0, 1f)]
    public float force;
    public float terminalX;

    public override void Enter() {
        Set(fall);
        animator.Play(jumpFall, 10, false, false);
        animator.Stop();
    }

    public override void Do() {
        base.Do();

        if (core.selfAwareness.grounded && core.velY <= 0) {
            Complete("grounded!");
        }

        SetAirSprite();
    }

    void HandleMovement() {
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f) {
            core.velX += h * force;
            core.FaceDirection(Vector2.right * h);
        }

        core.velX = Mathf.Clamp(core.velX, -terminalX, terminalX);
    }

    void SetAirSprite() {
        int frame = (int)Helpers.Map(core.velY, 4, -4, 0, jumpFall.count - 1, true);
        //animator.SeekToFrame(frame);
        animator.frame = frame;
    }

    public void Jump() {
        Set(jump, true);
    }

    public override void FixedDo() {
        HandleMovement();
        base.FixedDo();
    }

    public override void Exit() {
        base.Exit();
    }
}


