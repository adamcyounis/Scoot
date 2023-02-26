using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenu : MonoBehaviour {
    public AudioClip a_scoot;
    public AudioClip a_button;

    // Start is called before the first frame update
    void Start() {
        SoundSystem.system.PlaySFX(a_scoot);
    }

    // Update is called once per frame
    void Update() {

    }

    public void EnterArcade() {
        SoundSystem.system.PlaySFX(a_button);
        GameStateManager.manager.BeginArcadeMode();
    }

    public void EnterVersus() {
        SoundSystem.system.PlaySFX(a_button);
        GameStateManager.manager.BeginVersusMode();
    }

    public void EnterOptions() {
        SoundSystem.system.PlaySFX(a_button);

    }

    public void QuitGame() {

    }

}
