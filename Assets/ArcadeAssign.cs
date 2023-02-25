using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
public class ArcadeAssign : MonoBehaviour {

    public Button b;
    public TMP_Text text;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        int totalPlayers = GameSystem.system.characters.Where(x => x.life.team == 0).Count();

        if (!b.interactable) {
            if (totalPlayers > 0) {
                b.interactable = true;
                b.targetGraphic.color = new Color(1, 1, 1, 1f);
                text.color = new Color(1, 1, 1, 1f);
                text.text = "Continue";

            }
        } else {
            if (totalPlayers <= 0) {
                b.interactable = false;
                b.targetGraphic.color = new Color(1, 1, 1, 0.5f);
                text.color = new Color(1, 1, 1, 0.5f);
                text.text = "Awaiting Players";

            }
        }

    }
    public void Continue() {
        GameStateManager.manager.GoToNextArcadeLevel();
    }

}
