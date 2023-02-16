public class AnimationState : State {
    public Retro.Sheet sheet;
    public float fps;
    public bool loop;
    public override void Enter() {
        animator.Play(sheet, fps, loop, true);
        animator.finished.AddListener(RespondToFinished);
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

    void RespondToFinished(Retro.Sheet s) {
        if (s == sheet) {
            Complete("finished animation!");
        }
    }

}