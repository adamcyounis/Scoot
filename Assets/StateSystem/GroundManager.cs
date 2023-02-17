using UnityEngine;

public class GroundManager : State {
    public FallThrough fallThrough;
    public GroundControl standing;
    public CrouchControl crouching;
    public override void Enter() {
        base.Enter();
        state = null;
    }
    public override void Do() {
        base.Do();

        if (Input.GetAxis("Vertical") < -0.75f && core.selfAwareness.groundLayer.Equals("GroundFall")) {
            Set(fallThrough);
            core.velY = -4f;
        } else {

            if (!(state == fallThrough) || state.complete) {
                if (Input.GetAxis("Vertical") < -0.1f) {
                    Set(crouching);
                } else {
                    Set(standing);
                }

            }
        }


        if (!core.selfAwareness.grounded && state != fallThrough) {
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
