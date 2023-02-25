using UnityEngine;

public class AirDodge : State, AirState {
    public bool locked => time < 0.2f;
    public bool sliding => core.selfAwareness.grounded && core.velY <= 0;
    Vector2 direction;
    public AnimationCurve curve;
    public float speed = 3;
    bool usedDodge;
    public Retro.Sheet s_airDodge;

    public Retro.Sheet s_crouch;

    public bool startGrounded;
    private void Start() {
        core.selfAwareness.gotGrounded.AddListener(Grounded);
    }

    void Grounded() {
        usedDodge = false;
    }

    public override void Enter() {

        animator.Play(s_airDodge, 10, true, true);
        base.Enter();
        usedDodge = true;
        SetDirection();
    }

    void SetDirection() {
        Vector2 inputDirection = core.input.movement;

        if (inputDirection.magnitude > 0.1f) {
            direction = inputDirection.normalized;
            DustSpawner.spawner.BurstParticles(core.selfAwareness.footPoint, -direction * 2f, 8);

        } else {
            direction = Vector2.zero;
        }
    }
    public override void Do() {
        base.Do();
        if (time < 0.08f && direction == Vector2.zero) {
            SetDirection();
        }

        if (direction.y < 0 && core.selfAwareness.grounded) {
            animator.Play(s_crouch, 10, true, true);
        }


    }

    public override void FixedDo() {
        base.FixedDo();

        if (time < curve.keys[curve.keys.Length - 1].time) {
            if (!startGrounded) {
                body.velocity = direction * curve.Evaluate(time) * speed;
                if (core.selfAwareness.grounded && body.velocity.y < 0) {//align to ground to create wavedash
                    core.body.velocity = Vector2.right * Mathf.Sign(core.velX) * body.velocity.magnitude * 1.5f;
                }

            } else {
                core.body.velocity = Vector2.zero;

            }
        } else {
            Complete("finished time");
        }
    }

    public override void Exit() {
        base.Exit();
        startGrounded = false;
    }

    public bool CanAirDodge() {
        return core.state != this && !usedDodge;
    }

}
