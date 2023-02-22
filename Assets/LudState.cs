using UnityEngine;

public abstract class LudState : State {
    [HideInInspector]
    public Retro.RetroAnimator leftHandAnimator;

    [HideInInspector]
    public Retro.RetroAnimator rightHandAnimator;
    [HideInInspector]
    public Retro.RetroAnimator faceAnimator;
    [HideInInspector]
    public Animator anim;

}