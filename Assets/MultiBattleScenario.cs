using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattleScenario : MonoBehaviour {

    //set up how many enemies need to spawn in etc...

    public bool gameFinished;
    // Start is called before the first frame update
    void Start() {
        GameSystem.system.bwm.StartMatch();
    }

    // Update is called once per frame
    void Update() {
        bool prevGameFinished = gameFinished;

        //check whether the game should end. will set gameFinished to true

        if (gameFinished && !prevGameFinished) {
            GameSystem.system.bwm.EndMatch();
        }
    }
}
