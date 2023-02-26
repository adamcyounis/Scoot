using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroScreen : MonoBehaviour {
    bool transitioning;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (!transitioning && Time.time > 5) {
            transitioning = true;
            GameStateManager.manager.GoToTitle();
        }
    }
}
