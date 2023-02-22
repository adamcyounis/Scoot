using UnityEngine;

public class LudIdle : State {

    public float duration = 4;
    public override void Enter() {
        core.unityAnim.Play("Idle");
    }

    public override void Do() {
        base.Do();
        if (time > duration) {
            Complete("time expired!");
        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }

    public override void Exit() {
        base.Exit();
    }
}