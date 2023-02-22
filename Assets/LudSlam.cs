using System.Collections;
using System.Collections.Generic;
public class LudSlam : LudState {

    public Retro.Sheet s_slamHand;
    public override void Enter() {
        anim.Play("Slam");
        leftHandAnimator.Play(s_slamHand);
        leftHandAnimator.Stop();

        rightHandAnimator.Play(s_slamHand);
        rightHandAnimator.Stop();

    }


    public override void Do() {

        base.Do();
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
            Complete("done!");
        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }

    public override void Exit() {
        base.Exit();
    }
}
