using UnityEngine;

public class InputController : MonoBehaviour {
    public bool GetInputDown(string key) {
        return Input.GetButtonDown(key);
    }

    public bool GetInput(string key) {
        return Input.GetButton(key);
    }

}