using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : StateMachine {

    public AudioClip soundClip;
    public string title;
    protected Character core;
    protected Retro.RetroAnimator animator => core.animator;
    protected Rigidbody2D body => core.body;
    StateMachine parent;
    protected float time => Time.time - startTime;
    float startTime;
    Vector2 startPos;
    string endReason;

    public bool complete { get; private set; }

    public void Setup(StateMachine _parent) {
        parent = _parent;
        startTime = Time.time;
        startPos = core.position;
        complete = false;
        endReason = "";
    }

    public virtual void Enter() {

    }

    public virtual void Do() {
        if (state != null) {
            state.Do();
        }

    }

    public virtual void FixedDo() {
        if (state != null) {
            state.FixedDo();
        }

    }

    public virtual void Exit() {
        if (body != null) {
            core.ReturnToDefaultBodyProps();
        }
    }


    public void SetCore(Character newCore) {
        core = newCore;
    }
    protected void Complete(string reason = default) {
        endReason = reason;
        complete = true;
    }

    public void Sound() {

        if (soundClip != null) {
            SoundSystem.system.PlaySFX(soundClip);
        }
    }

    public new State GetDeepState() {
        return state != null ? state.GetDeepState() : this;
    }
}


public interface AirState {

}
