using UnityEngine;
using UnityEngine.Events;
public class SelfAwareness : MonoBehaviour {

    public UnityEvent gotGrounded = new UnityEvent();
    public Character core;
    public float footDepth;
    public float footWidth;
    float halfFootWidth => footWidth / 2f;

    public float groundDepth = 0.04f;
    Vector2 footPoint => core.position + (Vector2.down * footDepth);

    public bool grounded;
    public LayerMask groundMask;

    public string groundLayer;
    void Start() {

    }

    void Update() {

    }

    void FixedUpdate() {
        bool wasGrounded = grounded;
        grounded = IsGrounded() && core.body.velocity.y <= 0;

        if (grounded && !wasGrounded) {
            gotGrounded.Invoke();
        }
    }



    private bool IsGrounded() {
        Vector2 bottomLeft = footPoint + (Vector2.left * halfFootWidth) + (Vector2.down * groundDepth);
        Vector2 topRight = footPoint + (Vector2.right * halfFootWidth) + (Vector2.up * groundDepth);
        Collider2D[] colliders = Physics2D.OverlapAreaAll(bottomLeft, topRight, groundMask);

        if (colliders.Length > 0) {
            groundLayer = LayerMask.LayerToName(colliders[0].gameObject.layer);
        }

        return colliders.Length != 0;
    }


    private void OnDrawGizmos() {
        DrawFootline();
        DrawGroundCheck();
    }
    void DrawFootline() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(footPoint + Vector2.left * halfFootWidth, footPoint + Vector2.right * halfFootWidth);

    }

    void DrawGroundCheck() {
        Vector2 bottomLeft = footPoint + Vector2.left * halfFootWidth + Vector2.down * groundDepth;
        Vector2 topRight = footPoint + Vector2.right * halfFootWidth + Vector2.up * groundDepth;

        Helpers.DrawRect(new Rect(bottomLeft, topRight - bottomLeft));
    }
}