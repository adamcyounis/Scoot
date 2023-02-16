using System;
using UnityEngine;

public class AirControl : State {
    public Jump jump;
    public State fall;

    [Range(0, 1f)]
    public float force;
    public float terminalX;

    public override void Enter() {
        Set(fall);
    }

    public override void Do() {
        base.Do();

        if (core.selfAwareness.grounded && core.velY <= 0) {
            Complete("grounded!");
        }
    }

    void HandleMovement() {
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f) {
            core.velX += h * force;
            core.FaceDirection(Vector2.right * h);
        }

        core.velX = Mathf.Clamp(core.velX, -terminalX, terminalX);
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
