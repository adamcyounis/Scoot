using UnityEngine;
using System;
public class Coots : Character {


    [Header("States")]
    public GroundManager groundManager;
    public AirControl airControl;
    public AirDodge dodge;
    public Stun stun;
    private void Start() {
        Set(initState);
        GameManager.coots = this;

        life.hurtConfirmEvent.AddListener(GetHurt);
    }

    private void Update() {
        if (canMove || state.complete) {
            SetInputStates();
        }

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
        }
        if (airControl.jump.ShouldJump() && !(state == dodge && dodge.locked)) {
            Set(airControl, true);
            airControl.Jump();
            return;
        }


        if (state.complete) {
            if (!(state is AirState)) {
                if (!selfAwareness.grounded) {
                    Set(airControl);
                    return;
                }
            }

            if (selfAwareness.grounded) {
                Set(groundManager);
                return;
            }
        }

    }

    private void FixedUpdate() {
        if (!state.complete) {
            state.FixedDo();
        }
    }

    void GetHurt(CollisionInfo info) {
        Set(stun);
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

