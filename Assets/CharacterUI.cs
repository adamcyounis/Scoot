using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CharacterUI : MonoBehaviour {
    public List<GameObject> stockImages;
    public TMP_Text percentLabel;
    public TMP_Text nameLabel;
    public Image backPlate;
    public Character character;
    public Color color;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        UpdateStocks();
        nameLabel.text = character.characterName;
        percentLabel.text = character.percent.ToString() + "<size=60>%</size>";

        if (character.life.team >= 0 && character.life.team < GameSystem.system.teamColours.Count) {
            color = GameSystem.system.teamColours[character.life.team];
            backPlate.color = color;
            if (character.uiTag != null) {
                character.uiTag.color = color;

            }

        }


    }


    void UpdateStocks() {
        for (int i = 0; i < stockImages.Count; i++) {
            stockImages[i].SetActive(character.stocksRemaining > i);
        }

    }
}
