using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattleScenario : MonoBehaviour {
    GameStateManager m => GameStateManager.manager;

    public int enemies;
    public float dex;
    public float knockback;
    public Color color;
    //set up how many enemies need to spawn in etc...

    // Start is called before the first frame update
    void Start() {
        GameSystem.system.bwm.StartMatch();
        GameSystem.system.roundOver.AddListener(Finished);

        if (m.mode == GameStateManager.GameMode.Arcade) {
            for (int i = 0; i < enemies; i++) {
                GameSystem.system.AddEnemyCoots(1, dex, knockback, color);
            }

        }
    }

    // Update is called once per frame
    void Update() {

    }

    void Finished(Character c) {

        GameSystem.system.bwm.EndMatch(c.life.team == 0);
        /*
        if (m != null) {
            if (m.mode == GameStateManager.GameMode.Arcade) {
                if (c.life.team == 0) {
                    m.GoToNextArcadeLevel();
                } else {
                    m.GoToTitle();
                }
            } else {
                m.GoToTitle();
            }
        }
        */
    }

    private void OnDestroy() {
        if (GameSystem.system != null && GameSystem.system.roundOver != null) {
            GameSystem.system.roundOver.RemoveListener(Finished);
        }
    }
}
