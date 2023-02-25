using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BigWordsManager : MonoBehaviour {
    public AudioClip a_ready;
    public AudioClip a_go;
    public AudioClip a_gameSet;

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
        Time.timeScale = 0;
        image.enabled = true;
        image.sprite = ready;
        SoundSystem.system.PlaySFX(a_ready);
        StartCoroutine(PrepareToGo());
    }

    IEnumerator PrepareToGo() {
        yield return new WaitForSecondsRealtime(2);
        image.sprite = go;
        SoundSystem.system.PlaySFX(a_go);

        Time.timeScale = 1;
        yield return new WaitForSecondsRealtime(1);
        image.sprite = null;
        image.enabled = false;

    }

    public void EndMatch() {
        image.enabled = true;
        image.sprite = gameSet;
        SoundSystem.system.PlaySFX(a_gameSet);

        Time.timeScale = 0.1f;
    }
}
