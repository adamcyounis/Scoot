using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardingPlatform : MonoBehaviour {
    public static Color defaultColour = Color.red;
    public static Color boardedColour = Color.blue;

    public bool boarded = false;
    public SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start() {
        sprite.color = defaultColour;
    }

    // Update is called once per frame
    void Update() {

    }
    private void OnCollisionStay2D(Collision2D other) {
        if (
            GameManager.IsCoots(other.collider) &&
            !(GameManager.coots.state is AirControl) &&
            !boarded && GameManager.coots.velY <= 0 &&
            GameManager.coots.selfAwareness.grounded
        ) {
            Board();
        }
    }

    public void Board() {
        boarded = true;
        sprite.color = boardedColour;

    }
}
