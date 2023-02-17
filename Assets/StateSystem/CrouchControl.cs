using UnityEngine;

public class CrouchControl : State {

    public Retro.Sheet idle;
    public Retro.Sheet walk;
    [Range(0, 1f)]
    public float force;
    public float terminalX;
    public float drag = 0.9f;

    public override void Enter() {
        base.Enter();
        animator.Play(idle, 10, true, true);
    }
    public override void Do() {
        base.Do();

        HandleAnimation();


        if (Input.GetAxis("Vertical") >= 0) {
            Complete("released crouch");
        }

    }

    void HandleAnimation() {
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0) {
            animator.Play(walk, 10, true, false);
        } else {
            animator.Play(idle, 10, true, false);
        }

    }

    void HandleMovement() {
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f) {
            core.velX += h * force;
            core.FaceDirection(Vector2.right * h);
        } else {
            Debug.Log("decaying");
            core.velX *= drag;
        }

        core.velX = Mathf.Clamp(core.velX, -terminalX, terminalX);
    }

    public override void FixedDo() {
        base.FixedDo();
        HandleMovement();
    }
    public override void Exit() {
        base.Exit();
    }
}