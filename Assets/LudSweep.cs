public class LudSweep : LudState {
    public Retro.Sheet s_idleHand;

    public Retro.Sheet s_sweepHand;
    public override void Enter() {
        anim.Play("Sweep2");

        leftHandAnimator.Play(s_sweepHand);
        leftHandAnimator.Stop();

        rightHandAnimator.Play(s_idleHand);
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
