using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCounter : MonoBehaviour {
    public List<BoardingPlatform> platforms;
    public List<UnityEngine.UI.Image> platformImages;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        CheckPlatforms();
    }

    void CheckPlatforms() {
        for (int i = 0; i < platforms.Count; i++) {
            platformImages[i].gameObject.SetActive(!platforms[i].boarded);
        }
    }
}
