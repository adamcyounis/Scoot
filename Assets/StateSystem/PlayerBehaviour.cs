using UnityEngine;
using System;
public class PlayerBehaviour : Character {


    public GroundControl groundControl;
    public AirControl airControl;
    private void Update() {


        SetInputStates();

        if (state.complete) {
            //set a new state
        }

        if (!state.complete) {
            state.Do();
        }
    }
    void SetInputStates() {

    }

    private void FixedUpdate() {
        if (!state.complete) {
            state.FixedDo();
        }
    }
}
/*
public class GroundControl : State {
    public override void Enter() {
        base.Enter();
    }
    public override void Do() {
        base.Do();
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
    }

}
*/



