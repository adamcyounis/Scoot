using UnityEngine;

public class GroundManager : State {
    public GroundControl standing;
    public CrouchControl crouching;
    public override void Enter() {
        base.Enter();
    }
    public override void Do() {
        base.Do();
        if (Input.GetAxis("Vertical") < -0.1f) {
            Set(crouching);
        } else {
            Set(standing);
        }

        if (!core.selfAwareness.grounded) {
            Complete("left the ground");
        }
    }

    public override void FixedDo() {
        base.FixedDo();
    }
    public override void Exit() {
        base.Exit();
    }

}


