using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TItleMenu : MonoBehaviour {

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void EnterArcade() {
        Debug.Log("beginning arcade mode");
        GameStateManager.manager.BeginArcadeMode();
    }

    public void EnterVersus() {
        GameStateManager.manager.BeginVersusMode();

    }

    public void EnterOptions() {

    }

    public void QuitGame() {

    }

}
