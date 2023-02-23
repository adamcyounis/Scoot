using UnityEngine;
using System;
public class GroundControl : State {

    public Retro.Sheet idle;
    public Retro.Sheet walk;
    public Retro.Sheet run;
    [Range(0, 1f)]
    public float force;
    public float maxSprintX;
    public float maxWalkX;

    public float drag = 0.9f;

    public float baseWalkFPS;
    public override void Enter() {
        base.Enter();
    }
    public override void Do() {
        base.Do();

        HandleAnimation();

    }

    void HandleAnimation() {
        float h = core.input.movement.x;
        float g = Mathf.Abs(h);

        if (g > 0.01f && g < 0.4f) {
            animator.Play(walk, baseWalkFPS, true, false);
        } else if (g >= 0.4f) {
            animator.Play(run, baseWalkFPS, true, false);
        } else {
            animator.Play(idle, 10, true, false);
        }
    }

    void HandleMovement() {
        float h = core.input.movement.x;
        float g = Mathf.Abs(h);

        if (g > 0.01f && g < 0.5f) {
            core.velX += h * force;
            core.FaceDirection(Vector2.right * h);
            core.velX = Mathf.Clamp(core.velX, -maxWalkX, maxWalkX);

        } else if (g >= 0.5f) {
            core.velX += h * force * 2f;
            core.FaceDirection(Vector2.right * h);
            core.velX = Mathf.Clamp(core.velX, -maxSprintX, maxSprintX);

        } else {
            core.velX *= drag;
        }

    }

    public override void FixedDo() {
        base.FixedDo();
        HandleMovement();

    }
    public override void Exit() {
        base.Exit();
    }

}
