using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VersusAssign : MonoBehaviour {

    public Button b;
    public TMP_Text text;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        List<int> teams = new List<int>();
        foreach (Character c in GameSystem.system.characters) {
            if (!teams.Contains(c.life.team) && c.life.team >= 0) {
                teams.Add(c.life.team);
            }
        }
        bool ready = teams.Count > 1;

        if (!b.interactable) {
            if (ready) {
                b.interactable = true;
                b.targetGraphic.color = new Color(1, 1, 1, 1f);
                text.color = new Color(1, 1, 1, 1f);
                text.text = "Continue";

            }
        } else {
            if (!ready) {
                b.interactable = false;
                b.targetGraphic.color = new Color(1, 1, 1, 0.5f);
                text.color = new Color(1, 1, 1, 0.5f);
                text.text = "Awaiting Players";

            }
        }

    }

    public void Continue() {
        GameStateManager.manager.GoToBattleScene();
    }
}
