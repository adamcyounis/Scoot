using UnityEngine;

public class LudIdle : LudState {

    public float duration = 4;
    public Retro.Sheet s_idleHand;
    public override void Enter() {
        core.unityAnim.Play("Idle");
        leftHandAnimator.Play(s_idleHand);
        leftHandAnimator.Stop();

        rightHandAnimator.Play(s_idleHand);
        rightHandAnimator.Stop();
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