using System;
using System.Collections.Generic;
using UnityEngine;

public class ComboState : State {
    public List<State> comboStates;
    int comboIndex;
    public override void Enter() {
        base.Enter();
        if (comboIndex >= comboStates.Count) {
            comboIndex = 0;
        }
        if (comboIndex < comboStates.Count) {
            Set(comboStates[comboIndex], true);
            comboIndex++;
        }
    }

    public override void Do() {
        base.Do();
        if (state.complete) {
            comboIndex = 0;
            Complete("fell out of combo");
        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
    }
}