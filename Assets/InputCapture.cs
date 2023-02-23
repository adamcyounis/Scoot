using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputCapture : MonoBehaviour {

    public PlayerInput input;
    public bool jumpHeld;
    public bool jumpPressed;
    public bool attackHeld;
    public bool attackPressed;
    public bool specialHeld;
    public bool specialPressed;

    public bool shieldHeld;
    public bool shieldPressed;

    public bool startPressed;
    public bool selectPressed;


    public Vector2 movement;

    private void OnEnable() {
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //jump = input.actions["Move"].ReadValue<Vector2>();

        shieldHeld = input.actions["Shield"].IsPressed();
        shieldPressed = input.actions["Shield"].WasPressedThisFrame();

        attackHeld = input.actions["Attack"].IsPressed();
        attackPressed = input.actions["Attack"].WasPressedThisFrame();

        specialHeld = input.actions["Special"].IsPressed();
        specialPressed = input.actions["Special"].WasPressedThisFrame();

        jumpHeld = input.actions["Jump"].IsPressed();
        jumpPressed = input.actions["Jump"].WasPressedThisFrame();

        startPressed = input.actions["Start"].WasPressedThisFrame();
        selectPressed = input.actions["Select"].WasPressedThisFrame();

        movement = input.actions["Movement"].ReadValue<Vector2>();

    }
}
