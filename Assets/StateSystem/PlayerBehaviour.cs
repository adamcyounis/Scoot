using UnityEngine;
using System;
public class PlayerBehaviour : Character {

    [Header("States")]
    public GroundControl groundControl;
    public AirControl airControl;

    public AirDodge dodge;
    private void Start() {
        Set(initState);
    }

    private void Update() {
        SetInputStates();

        if (!state.complete) {
            state.Do();
        }
    }
    void SetInputStates() {
        if (state == airControl) {
            if (Input.GetButtonDown("Dodge") && dodge.CanAirDodge()) {
                Set(dodge);
                return;
            }
        } else {
            if (airControl.jump.inputtingJump && !(state == dodge && dodge.locked)) {
                Set(airControl);
                airControl.Jump();
                return;
            }
        }

        if (state.complete) {
            if (state == groundControl) {
                if (!selfAwareness.grounded) {
                    Set(airControl);
                }
            } else {
                if (selfAwareness.grounded) {
                    Set(groundControl);
                }
            }
        }
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

