using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingPlatform : MonoBehaviour {
    public Character character;
    Vector2 destinationPos;
    Vector2 startPos;
    SimpleAnimation descend;
    public float verticalOffset = 1.28f;
    public AnimationCurve curve;
    // Start is called before the first frame update
    void Awake() {
        if (descend == null) {
            Instantiate();
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (descend.animating) {
            descend.Update();
            transform.position = Vector2.Lerp(startPos, destinationPos, descend.value);
        } else {
            if (character.input.PressedAnything()) {
                DeSpawn();
            }
        }
    }

    public void SpawnIn(Vector2 spawnPosition) {
        gameObject.SetActive(true);
        if (descend == null) {
            Instantiate();
        }
        destinationPos = spawnPosition;
        startPos = destinationPos + Vector2.up * verticalOffset;
        transform.SetParent(null);
        transform.position = startPos;
        character.transform.position = transform.position + character.selfAwareness.inverseFootOffset;
        character.transform.SetParent(transform);
        descend.Animate(0, 1, true);
    }

    public void DeSpawn() {
        character.transform.SetParent(null);
        transform.SetParent(character.transform);
        gameObject.SetActive(false);
    }

    void Instantiate() {
        descend = new SimpleAnimation(0, 1, 0.5f, curve, true, false);
    }
}
