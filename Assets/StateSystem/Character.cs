using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Character : StateMachine {
    public Vector2 facingDirection => transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    public bool canMove = true;
    public bool canAttack = true;
    public InputController input;

    public Vector2 position => transform.position;
    public Rigidbody2D body => animator.rigidBody;
    public Animator unityAnim;
    public State initState;
    public Retro.RetroAnimator animator;
    public SelfAwareness selfAwareness;
    [HideInInspector]
    public Life life;
    public int stocksRemaining = 9;
    public string characterName;
    public float percent => life.percent;
    public Shaker shaker;

    public int controllerIndex;
    public BodyProps defaultBodyProps;
    public LoadingPlatform platform;
    public SpriteRenderer uiTag;
    public float velX {
        get {
            return body.velocity.x;
        }
        set {
            body.velocity = new Vector2(value, body.velocity.y);
        }
    }

    public float velY {
        get {
            return body.velocity.y;
        }
        set {
            body.velocity = new Vector2(body.velocity.x, value);
        }
    }

    public virtual void Awake() {
        if (body != null) {
            defaultBodyProps = new BodyProps(body.gravityScale);

        }
        animator.frameSet.AddListener(HandleFrameProperty);
        State[] states = GetComponentsInChildren<State>();
        foreach (State s in states) {
            s.SetCore(this);
        }

        if (input != null) {
            input.AssignCharacter(this);
        }
    }
    public void Start() {
        animator.boxManager.frameEvents.AddListener(Sound);
    }
    public void FaceDirection(Vector2 vector2) {
        transform.localScale = new Vector3(Mathf.Sign(vector2.x), 1);
    }

    public void ReturnToDefaultBodyProps() {

        body.gravityScale = defaultBodyProps.gravityScale;
        transform.up = Vector3.up;
    }

    void HandleFrameProperty(Retro.Sheet sheet, int frame) {

        Retro.Properties props = sheet.propertiesList[frame];
        //AnimationEffectPool.current.ApplyEffects(anim);

        bool x = false;
        bool y = false;
        Vector2 vel = new Vector2();

        foreach (Retro.BoxProperty b in props.frameProperties) {

            switch (b.name) {
                case "Event":
                    if (animator.boxManager.frameEvents.ContainsEvent(b.stringVal)) {
                        animator.boxManager.frameEvents.Invoke(b.stringVal);
                    }
                    break;
                case "Velocity":
                    vel = new Vector3(b.vectorVal.x, b.vectorVal.y);
                    break;
                case "velX":
                    x = true;
                    break;
                case "velY":
                    y = true;
                    break;
                case "Gravity":
                    body.gravityScale = b.floatVal;
                    break;
                case "CanAttack":
                    canAttack = b.boolVal;
                    break;
            }
        }

        if ((x || y)) {
            if (facingDirection == Vector2.left) vel.x = -vel.x;

            if (!x) vel.x = body.velocity.x;
            if (!y) vel.y = body.velocity.y;
            body.velocity = vel;
        }
    }

    public void Sound() {
        if (GetDeepState() != null) {
            GetDeepState().Sound();
        }
    }
}

public struct BodyProps {
    public BodyProps(float gravityScale_) {
        gravityScale = gravityScale_;
    }
    public readonly float gravityScale;

}