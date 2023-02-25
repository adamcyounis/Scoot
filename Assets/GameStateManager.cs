using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameStateManager : MonoBehaviour {
    [HideInInspector]
    public static GameStateManager manager;
    public enum GameMode { Arcade, Versus }
    public GameMode mode;
    public int arcadeSceneIndex = 0;
    public SceneReference assignScene;
    public SceneReference titleScene;
    public List<SceneReference> arcadeScenes;
    void Awake() {
        if (manager == null) {
            manager = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void BeginArcadeMode() {
        mode = GameMode.Arcade;
        arcadeSceneIndex = 0;
        Debug.Log("game state manager says hi");

        StartCoroutine(BeginSceneTransition(arcadeScenes[arcadeSceneIndex]));
    }


    public void BeginVersusMode() {
        mode = GameMode.Versus;
        BeginSceneTransition(assignScene);
    }

    public IEnumerator BeginSceneTransition(string nextScene) {
        yield return new WaitForSeconds(1);
        EndSceneTransition(nextScene);
    }

    void EndSceneTransition(string nextScene) {
        SceneManager.LoadScene(nextScene);
    }

}
