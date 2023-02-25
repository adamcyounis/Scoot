using UnityEngine;
using System;
public class Coots : Character {


    [Header("States")]
    public GroundManager groundManager;
    public AirControl airControl;
    public AirDodge dodge;
    public Stun stun;
    public ShieldState shield;

    public AttackStates attacks;
    public SpecialStates specials;


    public new void Start() {
        base.Start();
        Set(initState);
        GameManager.coots = this;
        life.hurtConfirmEvent.AddListener(GetHurt);
    }

    private void Update() {
        if (!animator.IsInHitStop()) {
            if ((canMove || state.complete)) {
                State prevState = state;
                if (canAttack) {
                    SetAttackStates();
                }

                if (state == prevState && canMove) { //haven't done an attack..
                    SetInputStates();
                }
            }

            if (!state.complete) {
                state.Do();
            } else {
                canAttack = true;
                canMove = true;
            }
        }
    }

    void SetAttackStates() {
        if (input.attackPressed) {
            Set(attacks, true);
        } else if (input.specialPressed) {
            Set(specials);
        }
    }

    void SetInputStates() {
        if (state == shield && input.movement.y < -0.5f) {
            dodge.startGrounded = true;
            Set(dodge);
            return;
        }

        if (state == airControl) {
            if (input.shieldHeld && dodge.CanAirDodge()) {
                Set(dodge);
                return;
            }
        }

        if (shield.ShouldShield()) {
            if (!(state == dodge && dodge.locked)) {
                Set(shield);
            }
        }

        if (airControl.jump.ShouldJump()) {
            if (!(state == dodge) || dodge.sliding) {

                Set(airControl, true);
                airControl.Jump();
                return;
            }
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
        if (!state.complete && !animator.IsInHitStop()) {
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

