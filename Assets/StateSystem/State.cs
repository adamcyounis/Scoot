using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State : StateMachine {
    Character core;
    StateMachine parent;
    float time => Time.time - startTime;
    float startTime;
    Vector2 startPos;
    public string endReason;
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

    }

    public virtual void FixedDo() {
    }

    public virtual void Exit() {

    }


    public void SetCore(Character newCore) {
        core = newCore;
    }
    void Complete(string reason = default) {
        endReason = reason;
        complete = true;
    }
}
