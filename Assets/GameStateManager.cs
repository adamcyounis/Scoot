using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameStateManager : MonoBehaviour {
    [HideInInspector]
    public static GameStateManager manager;
    public enum GameMode { Arcade, Versus }
    public GameMode mode;
    public int arcadeSceneIndex = 0;
    public SceneReference assignScene;
    public SceneReference titleScene;
    public List<SceneReference> arcadeScenes;
    public List<Sprite> transitionSprites;
    public Image transitionWipeImage;
    SimpleAnimation flipBook;
    void Awake() {
        if (manager == null) {
            manager = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            flipBook = new SimpleAnimation(0, transitionSprites.Count, 2, SimpleAnimation.Curve.Linear, false, true);
        } else {
            Destroy(gameObject);
        }

    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (flipBook.animating) {
            flipBook.Update();
            int frame = (int)flipBook.value;
            frame = Mathf.Clamp(frame, 0, transitionSprites.Count - 1);
            transitionWipeImage.sprite = transitionSprites[frame];
        }
        transitionWipeImage.enabled = flipBook.animating;

    }

    public void GoToTitle() {
        StartCoroutine(BeginSceneTransition(titleScene));
    }


    public void ResetScene() {
        StartCoroutine(BeginSceneTransition(SceneManager.GetActiveScene().name));
    }


    public void BeginArcadeMode() {
        mode = GameMode.Arcade;
        arcadeSceneIndex = 0;
        StartCoroutine(BeginSceneTransition(arcadeScenes[arcadeSceneIndex]));
    }


    public void BeginVersusMode() {
        mode = GameMode.Versus;
        BeginSceneTransition(assignScene);
    }

    public IEnumerator BeginSceneTransition(string nextScene) {
        flipBook.Animate(true);
        yield return new WaitForSecondsRealtime(1);
        EndSceneTransition(nextScene);
    }

    void EndSceneTransition(string nextScene) {
        SceneManager.LoadScene(nextScene);
        Time.timeScale = 1;

    }

}
