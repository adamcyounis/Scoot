using System;
using UnityEngine;

public class Character : StateMachine {
    public Vector2 facingDirection => transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    public bool canMove;
    public bool canAttack;
    public InputController input;

    public Vector2 position => transform.position;
    public Rigidbody2D body => animator.rigidBody;
    public State initState;
    public Retro.RetroAnimator animator;
    public SelfAwareness selfAwareness;
    [HideInInspector]
    public Life life;
    public int stocksRemaining = 9;
    public string characterName;
    public float percent => life.percent;

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

    private void Awake() {
        State[] states = GetComponentsInChildren<State>();
        foreach (State s in states) {
            s.SetCore(this);

        }
    }

    public void FaceDirection(Vector2 vector2) {
        transform.localScale = new Vector3(Mathf.Sign(vector2.x), 1);
    }
}
