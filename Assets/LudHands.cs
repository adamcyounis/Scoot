using System.Collections;
using System.Collections.Generic;

//LudHands
//Boss Character
/*
    - Features
    - Each of the hands operates together in a single Behaviour State
    - Each state defines things that Hand 1 will be doing and Hand 2
    - Two Separate Life Components, Two separate Animators

    - Basic Attacks..
        - Should be ludwig-themed if possible
        - e.g. slamming the desk while saying "BOYS"
        - Doing the lud
        - Will need to Revisit Clips of ludwig to mine for content


*/

public class LudHands : Character {

    public LudIdle idle;
    public LudSlam slam;

    // Start is called before the first frame update
    void Start() {
        Set(idle);
    }

    // Update is called once per frame
    public void Update() {
        if (!state.complete) {
            state.Do();
        } else {
            if (state == idle) {
                Set(slam);
            } else {
                Set(idle);
            }
        }
    }
    public void FixedUpdate() {
        if (!state.complete) {
            state.FixedDo();
        }
    }

}
