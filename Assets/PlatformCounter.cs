using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlatformCounter : MonoBehaviour {
    public List<BoardingPlatform> platforms;
    public List<UnityEngine.UI.Image> platformImages;
    float startTime;
    public TMP_Text timerLabel;
    float elapsedTime => ongoingTime - startTime;
    float ongoingTime;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        CheckPlatforms();
        int minutes = (int)elapsedTime / 60;
        int seconds = (int)elapsedTime % 60;
        int hundredths = (int)(elapsedTime * 100) % 100;
        string minString = ((minutes < 10) ? "0" : "") + minutes.ToString();
        string secString = ((seconds < 10) ? "0" : "") + seconds.ToString();
        timerLabel.text = minString + "\'" + secString + "\"" + hundredths.ToString();

    }

    void CheckPlatforms() {
        int boardedPlatforms = 0;
        for (int i = 0; i < platforms.Count; i++) {
            bool b = platforms[i].boarded;
            platformImages[i].gameObject.SetActive(!b);
            if (b) {
                boardedPlatforms++;
            }
        }

        ongoingTime = boardedPlatforms == platforms.Count ? ongoingTime : Time.time;

    }

    private void OnEnable() {
        startTime = Time.time;
    }
}
