using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenu : MonoBehaviour {
    public AudioClip a_scoot;
    // Start is called before the first frame update
    void Start() {
        SoundSystem.system.PlaySFX(a_scoot);
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
