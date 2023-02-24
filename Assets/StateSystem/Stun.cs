public class Stun : State {

    public State hurt;
    public State airHurt;
    public bool startedGrounded;
    public float stunTime = 0.3f;
    float stunDrag = 0.97f;
    public float stunLockSpeed = 4f;
    public override void Enter() {
        base.Enter();
        startedGrounded = core.selfAwareness.grounded && body.velocity.y <= 0;
        Set(startedGrounded ? hurt : airHurt, true);
        stunTime = 0.3f + core.life.percent * 0.002f;
        core.canMove = false;
    }
    public override void Do() {
        base.Do();
        /*
        if (core.selfAwareness.grounded && !startedGrounded) {
            Complete("landed");
        }
*/
        bool flyingAway = body.velocity.magnitude > stunLockSpeed && body.velocity.y > 0;
        if (state.complete || (time > stunTime && !flyingAway)) {
            Complete("finished!");
            core.canMove = true;
        }
    }

    public override void FixedDo() {
        base.FixedDo();
        body.velocity *= stunDrag;

    }
    public override void Exit() {
        base.Exit();
        core.canMove = true;

    }

}
