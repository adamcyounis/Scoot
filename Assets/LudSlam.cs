using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LudSlam : State {

    public override void Enter() {
        core.unityAnim.Play("Slam");
    }


    public override void Do() {

        base.Do();
        if (core.unityAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
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