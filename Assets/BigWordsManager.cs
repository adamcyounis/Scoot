using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BigWordsManager : MonoBehaviour {

    public Sprite ready;
    public Sprite go;
    public Sprite gameSet;
    public Image image;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void StartMatch() {
        image.enabled = true;
        image.sprite = ready;
        StartCoroutine(PrepareToGo());
    }

    IEnumerator PrepareToGo() {
        yield return new WaitForSeconds(1);
        image.sprite = go;

        yield return new WaitForSeconds(1);
        image.sprite = null;
        image.enabled = false;

    }

    public void EndMatch() {
        image.enabled = true;

        image.sprite = gameSet;
    }
}
