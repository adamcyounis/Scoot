using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CharacterUI : MonoBehaviour {
    public List<GameObject> stockImages;
    public TMP_Text percentLabel;
    public TMP_Text nameLabel;
    public Character character;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        UpdateStocks();
        nameLabel.text = character.characterName;
        percentLabel.text = character.percent.ToString() + "<size=30>%</size>";
    }

    void UpdateStocks() {
        for (int i = 0; i < stockImages.Count; i++) {
            stockImages[i].SetActive(character.stocksRemaining > i);
        }

    }
}
