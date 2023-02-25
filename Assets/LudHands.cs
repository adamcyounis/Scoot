using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public LudSweep sweep;
    public LudSalute salute;

    public Retro.RetroAnimator leftHandAnimator;
    public Retro.RetroAnimator rightHandAnimator;
    public Retro.RetroAnimator faceAnimator;
    public Animator anim;
    State previousAttack;
    public float maxHealth;
    bool defeated = false;
    public AudioClip a_slap;
    public AudioClip a_punch;
    public AudioClip a_crunch;


    public override void Awake() {
        base.Awake();
        LudState[] ludStates = GetComponentsInChildren<LudState>();
        foreach (LudState l in ludStates) {
            l.leftHandAnimator = leftHandAnimator;
            l.rightHandAnimator = rightHandAnimator;
            l.faceAnimator = faceAnimator;
            l.anim = anim;
        }
    }

    // Start is called before the first frame update
    void Start() {
        leftHandAnimator.boxManager.collisionEvent.AddListener(life.HandleCollisionEvent);
        rightHandAnimator.boxManager.collisionEvent.AddListener(life.HandleCollisionEvent);
        leftHandAnimator.boxManager.hitConfirmEvent.AddListener(HitConfirm);

        Set(idle);
        GameSystem.system.bwm.StartMatch();
    }

    // Update is called once per frame
    public void Update() {
        if (!state.complete) {
            state.Do();
        } else {

            if (state == idle) {
                List<State> attacks = new List<State>() { salute, sweep, slam };
                attacks.Remove(previousAttack);
                int r = Random.Range(0, attacks.Count);
                Set(attacks[r]);
                previousAttack = state;

            } else {
                Set(idle);
            }
        }

        if (life.percent > maxHealth && !defeated) {
            defeated = true;
            Win();
        }
    }
    public void FixedUpdate() {
        if (!state.complete) {
            state.FixedDo();
        }
    }

    public void Shake() {
        GameManager.gm.shaker.Shake();
    }

    void Win() {
        GameSystem.system.bwm.EndMatch();
        GameStateManager.manager.GoToNextArcadeLevel();
    }

    void SlamSound() {
        SoundSystem.system.PlaySFX(a_crunch);
    }

    void HitConfirm(CollisionInfo col) {
        if (col.collision.collider.GetAnimator().GetSheet() == sweep.s_sweepHand) {
            SoundSystem.system.PlaySFX(a_slap);
        }

        if (col.collision.collider.GetAnimator().GetSheet() == slam.s_slamHand) {
            SoundSystem.system.PlaySFX(a_crunch);
        }

        if (col.collision.collider.GetAnimator().GetSheet() == salute.s_saluteHand) {
            SoundSystem.system.PlaySFX(a_punch);
        }

    }

}
