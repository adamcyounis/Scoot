using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardThePlatformsManager : MonoBehaviour {

    public List<BoardingPlatform> platforms;
    bool finished;
    // Start is called before the first frame update
    void Start() {
        Time.timeScale = 0;
        GameSystem.system.bwm.StartMatch();
    }

    // Update is called once per frame
    void Update() {
        if (!finished) {
            int boarded = 0;
            foreach (BoardingPlatform p in platforms) {
                if (p.boarded) {
                    boarded++;
                }
            }

            if (boarded == platforms.Count) {
                GameSystem.system.bwm.EndMatch(true);
                finished = true;
            }
        }
    }
}
