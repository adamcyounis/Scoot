public class AnimationState : State {
    public Retro.Sheet sheet;
    public float fps;
    public bool loop;
    public float duration;
    public override void Enter() {
        animator.Play(sheet, fps, loop, true);
        animator.finished.AddListener(RespondToFinished);
    }
    public override void Do() {
        base.Do();

        if (loop && duration != 0) {
            if (time > duration) {
                Complete("Finished duration!");
            }
        }
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