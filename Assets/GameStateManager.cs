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
    public SceneReference arcadeAssign;

    public SceneReference versusAssign;
    public SceneReference titleScene;
    public SceneReference battleScene;
    public List<SceneReference> arcadeScenes;
    public List<Sprite> transitionSprites;
    public Image transitionWipeImage;
    SimpleAnimation flipBook;
    public bool addingPlayersLocked => SceneManager.GetActiveScene().path != arcadeAssign.ScenePath && SceneManager.GetActiveScene().path != versusAssign.ScenePath;
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

    public void GoToNextArcadeLevel() {
        arcadeSceneIndex++;
        StartCoroutine(BeginSceneTransition(arcadeScenes[arcadeSceneIndex]));
    }

    public void GoToNextScene(bool won) {

        if (mode == GameMode.Arcade && won) {
            GoToNextArcadeLevel();
        } else {
            GoToTitle();
        }
    }

    public void GoToBattleScene() {
        StartCoroutine(BeginSceneTransition(battleScene));
    }

    public void BeginVersusMode() {
        mode = GameMode.Versus;
        BeginSceneTransition(arcadeAssign);
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
