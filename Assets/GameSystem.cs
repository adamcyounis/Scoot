using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Linq;
public class GameSystem : MonoBehaviour {

    public static GameSystem system;
    public PlayerInputManager manager;
    public GameObject cootsPrefab;
    public GameObject enemyCootsPrefab;

    public Transform nameplateWrapper;
    public GameObject namePlatePrefab;
    public List<PlayerInput> inputModules;
    public Dictionary<PlayerInput, int> playerInputTeams = new Dictionary<PlayerInput, int>();


    public UnityEvent<Transform> newCharacter = new UnityEvent<Transform>();
    public DustSpawner dust;
    public HitEffectPool pool;
    Level level;
    public List<Color> teamColours;
    public BigWordsManager bwm;
    public PauseMenu pauseMenu;
    public SceneReference titleScreen;

    [HideInInspector]
    public List<Coots> characters;
    public AudioClip s_playerDefeated;

    [HideInInspector]
    public UnityEvent<Character> roundOver = new UnityEvent<Character>();

    public List<AudioClip> callouts;
    private void Awake() {
        if (system == null) {
            system = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            level = GameObject.FindObjectOfType<Level>();
            DustSpawner.spawner = dust;
            HitEffectPool.p = pool;
            manager.gameObject.SetActive(true);
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
        if (!GameStateManager.manager.addingPlayersLocked) {
            AddInputModule(p);
            Coots c = AddCoots(p, inputModules.Count);

        }
    }

    public Character AddEnemyCoots(int team, float reactionTime, float knockbackMod, Color color) {
        Coots newCoots = GameObject.Instantiate(enemyCootsPrefab).GetComponent<Coots>();
        newCoots.life.team = team;

        characters.Add(newCoots);
        AddCharacterUI(newCoots);

        if (level != null) {
            level.SpawnCharacter(newCoots);
        } else {
            newCoots.transform.position = Vector2.up * 1f;
        }
        newCoots.animator.spriteRenderer.color = color;
        newCoots.life.kbFactor = knockbackMod;
        if (newCoots.input is CPUInputController c) {
            c.dexterity = reactionTime;
        }
        return newCoots;
    }

    void AddInputModule(PlayerInput p) {
        p.transform.SetParent(transform);
        inputModules.Add(p);
    }

    Coots AddCoots(PlayerInput p, int index) {
        Coots newCoots = GameObject.Instantiate(cootsPrefab).GetComponent<Coots>();
        newCoots.input = p.GetComponent<InputController>();
        newCoots.input.input = p;
        newCoots.controllerIndex = index;
        newCoots.life.team = index;

        characters.Add(newCoots);
        AddCharacterUI(newCoots);

        if (!playerInputTeams.ContainsKey(p)) {
            playerInputTeams.Add(p, index);

        }

        if (level != null) {
            level.SpawnCharacter(newCoots);

        } else {
            newCoots.transform.position = Vector2.up * 1f;
        }

        return newCoots;
    }

    void AddCharacterUI(Character c) {
        GameObject newNameplate = GameObject.Instantiate(namePlatePrefab, Vector2.zero, Quaternion.identity, nameplateWrapper);
        CharacterUI nameplateScript = newNameplate.GetComponent<CharacterUI>();
        nameplateScript.character = c;
        newCharacter.Invoke(c.transform);

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.path == titleScreen.ScenePath) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        } else {
            LoadPlayers();
            level = GameObject.FindObjectOfType<Level>();

        }
    }

    void LoadPlayers() {

        if (nameplateWrapper != null && nameplateWrapper.transform.childCount > 0) {
            foreach (Transform child in nameplateWrapper.transform) {
                if (child.gameObject != null) {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        characters.Clear();

        for (int i = inputModules.Count - 1; i >= 0; i--) {
            if (inputModules[i] != null) {
                AddCoots(inputModules[i], playerInputTeams[inputModules[i]]);
            } else {
                inputModules.Remove(inputModules[i]);
            }
        }
    }

    public void PlayerDefeated(Character ch) {

        List<int> teamsRemaining = new List<int>();
        foreach (Character c in characters) {
            if (!teamsRemaining.Contains(c.life.team) && c.stocksRemaining > 0) {
                teamsRemaining.Add(c.life.team);
            }
        }

        if (teamsRemaining.Count > 1) {
            SoundSystem.system.PlaySFX(s_playerDefeated);
        } else {
            roundOver.Invoke(characters.FirstOrDefault(x => x.stocksRemaining > 0));
        }
    }

}
