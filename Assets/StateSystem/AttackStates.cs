public class AttackStates : State {
    public State neutralAttack;
    public State downAttack;


    public override void Enter() {
        base.Enter();

        if (core.input.movement.y < 0) {
            Set(downAttack, true);
        } else {
            Set(neutralAttack, true);
        }
        core.canAttack = false;
    }

    public override void Do() {
        base.Do();
        if (state.complete) {
            Complete("finished");
        }
    }

    public override void FixedDo() {
        base.FixedDo();
        core.velX *= 0.95f;
    }
    public override void Exit() {
        base.Exit();
        core.canAttack = true;

    }
}
