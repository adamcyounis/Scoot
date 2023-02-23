using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour {

    public static GameManager gm;
    public static Coots coots;
    public ActionCamera cam;
    public Shaker shaker;

    float hitStopTime;
    float hitStopDuration;
    bool hitStopping;
    private void Awake() {
        gm = this;
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (hitStopping) {
            HandleHitStop();
        }

        if (Input.GetKeyDown(KeyCode.Home)) {
            Debug.Log("reload!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void HandleHitStop() {
        if (Time.unscaledTime - hitStopTime > hitStopDuration) {
            Time.timeScale = 1;
            hitStopping = false;
        }
    }

    public void HitStop(float t = 0.12f) {
        hitStopTime = Time.unscaledTime;
        hitStopDuration = t;
        hitStopping = true;
        Time.timeScale = 0;
    }

    public static bool IsCoots(Collider2D col) {
        if (col.transform.root.GetComponentInChildren<Coots>() is Coots c) {
            return true;
        }
        return false;
    }
}
