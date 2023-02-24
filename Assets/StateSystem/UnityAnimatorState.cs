using System;
using UnityEngine;
public class UnityAnimatorState : State {
    public Retro.Sheet sheet;
    public bool applyRootMotion = true;
    public override void Enter() {
        core.unityAnim.Play(title);
        animator.Play(sheet);
        animator.Stop();
        base.Enter();
        core.unityAnim.applyRootMotion = applyRootMotion;
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

        core.unityAnim.applyRootMotion = false;

    }
}

