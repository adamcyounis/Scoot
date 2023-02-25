using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSwitcher : MonoBehaviour {
    public int team;
    public BoxCollider2D switcher;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.transform.root.GetComponentInChildren<Life>() is Life l) {
            l.team = team;
            GameSystem.system.playerInputTeams[l.character.input.input] = team;
        }
    }
}
