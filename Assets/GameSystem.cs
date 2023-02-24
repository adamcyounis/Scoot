using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public class GameSystem : MonoBehaviour {

    public static GameSystem system;
    public PlayerInputManager manager;
    public GameObject cootsPrefab;
    public Transform nameplateWrapper;
    public GameObject namePlatePrefab;
    public List<PlayerInput> inputModules;
    public UnityEvent<Transform> newCharacter = new UnityEvent<Transform>();
    private void Awake() {
        if (system == null) {
            system = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

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

    public void OnPlayerJoined(PlayerInput p) {
        AddInputModule(p);
        Coots c = AddCoots(p, inputModules.Count);
    }

    void AddInputModule(PlayerInput p) {
        p.transform.SetParent(transform);
        inputModules.Add(p);
    }

    Coots AddCoots(PlayerInput p, int index) {
        Coots newCoots = GameObject.Instantiate(cootsPrefab).GetComponent<Coots>();
        newCoots.transform.position = Vector2.up * 1f;
        newCoots.input = p.GetComponent<InputController>();
        newCoots.input.input = p;
        newCoots.controllerIndex = index;
        newCoots.life.team = index;

        AddCharacter(newCoots);

        return newCoots;
    }

    void AddCharacter(Character c) {
        GameObject newNameplate = GameObject.Instantiate(namePlatePrefab);
        CharacterUI nameplateScript = newNameplate.GetComponent<CharacterUI>();
        nameplateScript.character = c;
        newNameplate.transform.SetParent(nameplateWrapper);
        newCharacter.Invoke(c.transform);

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        LoadPlayers();
    }

    void LoadPlayers() {
        foreach (Transform child in nameplateWrapper.transform) {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < inputModules.Count; i++) {
            AddCoots(inputModules[i], i);
        }
    }




}
