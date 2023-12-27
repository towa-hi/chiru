using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager ins;
    public bool isMenu;
    public bool isPaused;
    public MainMenuUI menuUI;
    public GameObject playerPrefab;
    public GameObject enemyTheoPrefab;
    public GameObject enemyTurretPrefab;
    public GameObject enemyCapipiPrefab;

    public List<Spawner> spawners;

    public AudioSource music;
    
    public GameObject cursor;
    public GameObject player;
    bool gameWon = false;
    void Awake()
    {
        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (this != ins)
            {
                Destroy(gameObject);
            }
        }
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            LoadLevel("GameScene");
        }
    }

    void Start()
    {
        SetIsMenu(true);
    }

    public void SetIsMenu(bool menu)
    {
        isMenu = menu;
        menuUI.Activate(menu);
    }

    public void PauseGame(bool isPaused)
    {
        
    }

    public void NewGame()
    {
        SetIsMenu(false);
        LoadLevel("GameScene");
    }

    void Update()
    {
        if (gameWon)
        {
            return;
        }
        if (level != 0 && enemiesThisLevel.TrueForAll(enemy => !enemy || enemy.isDead))
        {
            if (level < maxLevel)
            {
                SetLevel(level + 1);
            }
            else
            {
                // Show win message
                Debug.Log("You won the game!");
                OnWin();
            }
        }
    }

    void OnWin()
    {
        GuiController guiController = FindRootObjectWithComponent<GuiController>();
        guiController.OnWin();
        music.mute = true;
        Time.timeScale = 0;
    }
    public void OnDeath()
    {
        // show death canvas
        SoundPlayer soundPlayer = GetComponent<SoundPlayer>();
        soundPlayer.PlaySound("playerDeath");
        // find a root object with component GuiController
        // do guiController.OnDeath()
        GuiController guiController = FindRootObjectWithComponent<GuiController>();
        guiController.OnDeath();
        Time.timeScale = 0;
    }
    
    T FindRootObjectWithComponent<T>() where T : Component
    {
        T[] components = FindObjectsOfType<T>();
        foreach (T component in components)
        {
            if (component.transform.parent == null) // Check if the object is a root object
            {
                return component;
            }
        }
        return null; // Return null if no root object with the component is found
    }
    public void OnRestart()
    {
        LoadLevel("GameScene");
    }
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadSceneAsync(levelName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (!rootObj.activeInHierarchy)
                {
                    continue;
                }
                if (rootObj.GetComponent<Cursor>())
                {
                    cursor = rootObj;
                }
                if (rootObj.GetComponent<PlayerSpawner>())
                {
                    Debug.Log("Spawning player");
                    player = Instantiate(playerPrefab, rootObj.transform.position, Quaternion.identity);
                }
                else if (rootObj.GetComponent<Spawner>())
                {
                    spawners.Add(rootObj.GetComponent<Spawner>());
                }
            }
            SetLevel(1);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    int maxLevel = 4;
    public int level = 0;
    public List<Entity> enemiesThisLevel;
    public void SetLevel(int newLevel)
    {
        level = newLevel;
        switch (newLevel)
        {
            case 1:
                RandomlySpawn(3, 0);
                break;
            case 2:
                RandomlySpawn(5, 1);
                break;
            case 3:
                RandomlySpawn(8, 2);
                break;
            case 4:
                RandomlySpawn(12, 4);
                break;
        }
    }

    public void RandomlySpawn(int theos, int capipis)
    {
        if (player == null)
        {
            Debug.LogError("Player object not found.");
            return;
        }

        // Sort spawners based on distance from the player (farthest first)
        spawners.Sort((a, b) => 
            Vector3.Distance(b.transform.position, player.transform.position)
                .CompareTo(Vector3.Distance(a.transform.position, player.transform.position)));

        HashSet<Spawner> selectedSpawners = new HashSet<Spawner>();

        // Spawn Theos
        for (int i = 0; i < theos; i++)
        {
            Spawner spawner = GetRandomSpawner(selectedSpawners);
            if (spawner != null)
            {
                GameObject theo = Instantiate(enemyTheoPrefab, spawner.spawnPoint.transform.position, Quaternion.identity);
                enemiesThisLevel.Add(theo.GetComponent<Entity>());
                selectedSpawners.Add(spawner);
            }
        }

        // Spawn Capipis
        for (int i = 0; i < capipis; i++)
        {
            Spawner spawner = GetRandomSpawner(selectedSpawners);
            if (spawner != null)
            {
                GameObject capipi = Instantiate(enemyCapipiPrefab, spawner.spawnPoint.transform.position, Quaternion.identity);
                enemiesThisLevel.Add(capipi.GetComponent<Entity>());
                selectedSpawners.Add(spawner);
            }
        }
    }

// Helper method to get a random spawner that hasn't been selected yet
    Spawner GetRandomSpawner(HashSet<Spawner> selectedSpawners)
    {
        List<Spawner> availableSpawners = new List<Spawner>(spawners);
        availableSpawners.RemoveAll(s => selectedSpawners.Contains(s));

        if (availableSpawners.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSpawners.Count);
            return availableSpawners[randomIndex];
        }

        return null;
    }
    
    public void ContinueSavedGame()
    {
        
    }

    public void SaveGame()
    {
        
    }

    public void HandlePlayerDeath()
    {
        Debug.Log("Player died, reset game");
    }
    
    public void QuitGame()
    {
        #if UNITY_STANDALONE
        Application.Quit();
        #endif
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
