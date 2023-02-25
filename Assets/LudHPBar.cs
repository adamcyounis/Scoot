using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LudHPBar : MonoBehaviour {
    public LudHands lud;
    public Image bar;
    float maxSize;
    // Start is called before the first frame update
    void Start() {
        maxSize = bar.rectTransform.sizeDelta.x;
    }

    // Update is called once per frame
    void Update() {
        float normalized = ((lud.maxHealth - lud.life.percent) / lud.maxHealth);
        normalized = Mathf.Clamp01(normalized);
        bar.rectTransform.sizeDelta = new Vector2(maxSize * normalized, bar.rectTransform.sizeDelta.y);
    }
}
