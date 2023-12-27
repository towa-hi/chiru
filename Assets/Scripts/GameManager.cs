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
    
    public GameObject cursor;
    public GameObject player;
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
            }
        }
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
                RandomlySpawn(0, 1);
                break;
            case 2:
                RandomlySpawn(4, 1);
                break;
            case 3:
                RandomlySpawn(8, 3);
                break;
            case 4:
                RandomlySpawn(12, 6);
                break;
        }
    }

    public void RandomlySpawn(int theos, int capipis)
    {
        List<Spawner> availableSpawners = new List<Spawner>(spawners);
        for (int i = 0; i < theos + capipis; i++)
        {
            if (availableSpawners.Count == 0) break;

            int spawnerIndex = Random.Range(0, availableSpawners.Count);
            Spawner selectedSpawner = availableSpawners[spawnerIndex];
            availableSpawners.RemoveAt(spawnerIndex);

            GameObject enemyPrefab = i < theos ? enemyTheoPrefab : enemyCapipiPrefab;
            GameObject enemy = Instantiate(enemyPrefab, selectedSpawner.spawnPoint.transform.position, Quaternion.identity);
            enemiesThisLevel.Add(enemy.GetComponent<Entity>());
        }
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
