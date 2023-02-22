using System;
using UnityEngine;
public class LudSalute : LudState {
    public Retro.Sheet s_idleHand;

    public Retro.Sheet s_saluteHand;
    public override void Enter() {
        anim.Play("Salute");

        leftHandAnimator.Play(s_idleHand);
        leftHandAnimator.Stop();

        rightHandAnimator.Play(s_saluteHand);
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
