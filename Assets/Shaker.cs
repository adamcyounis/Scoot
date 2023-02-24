using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour {
    float startTime = 0;
    float time => Time.unscaledTime - startTime;
    public float duration = 0.3f;
    public bool animating;
    public float amount;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (animating) {
            float value = (1 - (time / duration)) * amount;
            float x = Random.Range(-value, value);
            float y = Random.Range(-value, value);
            transform.localPosition = new Vector2(x, y);
            if (time > duration) {
                animating = false;
            }
        } else {
            transform.localPosition = Vector2.zero;
        }
    }

    public void Shake(float amount_ = 0.2f, float duration_ = 0.12f) {
        startTime = Time.unscaledTime;
        duration = duration_;
        amount = amount_;
        animating = true;
    }
}
