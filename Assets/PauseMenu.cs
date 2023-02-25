using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PauseMenu : MonoBehaviour {

    public bool isOpen => container.activeInHierarchy;
    public GameObject container;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void ToggleMenu() {
        if (isOpen) {
            CloseMenu();
        } else {
            OpenMenu();
        }
    }

    public void OpenMenu() {
        container.SetActive(true);
        Time.timeScale = 0;
    }

    public void CloseMenu() {
        container.SetActive(false);
        Time.timeScale = 1;
    }

    public void PressResumeButton() {
        CloseMenu();
    }

    public void PressRestartButton() {
        GameStateManager.manager.ResetScene();
        container.SetActive(false);

    }
    public void PressQuitToTitleButton() {
        GameStateManager.manager.GoToTitle();
        container.SetActive(false);


    }
}
