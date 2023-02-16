using UnityEngine;

public class SelfAwareness : MonoBehaviour {
    public Character core;
    public float footDepth;
    public float footWidth;
    public float groundDepth = 0.04f;
    Vector2 footPoint => core.position + (Vector2.down * footDepth);

    public bool grounded;
    public LayerMask groundMask;
    void Start() {

    }

    void Update() {

    }

    void FixedUpdate() {
        grounded = IsGrounded();

    }



    private bool IsGrounded() {
        RaycastHit2D ray = Physics2D.Raycast(footPoint, Vector2.down, groundDepth, groundMask);
        return ray.collider != null;
    }


    private void OnDrawGizmos() {
        DrawFootline();
    }
    void DrawFootline() {
        float halfFootWidth = footWidth / 2f;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(footPoint + Vector2.left * halfFootWidth, footPoint + Vector2.right * halfFootWidth);

    }
}