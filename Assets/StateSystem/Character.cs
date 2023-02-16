using UnityEngine;

public class Character : StateMachine {
    public Vector2 facingDirection => transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    public bool canMove;
    public bool canAttack;
    public InputController input;

    public Vector2 position => transform.position;
    public Rigidbody2D body;
    public State initState;
    private void Awake() {
        State[] states = GetComponentsInChildren<State>();
        foreach (State s in states) {
            s.SetCore(this);

        }
    }
}


public class InputController : MonoBehaviour {
    public bool GetInputDown(string key) {
        return Input.GetButtonDown(key);
    }

    public bool GetInput(string key) {
        return Input.GetButton(key);
    }

}