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
        Debug.Log("baba boey");
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

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadSceneAsync(levelName);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            foreach (GameObject rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Debug.Log(rootObj.name);
                if (rootObj.GetComponent<Cursor>())
                {
                    cursor = rootObj;
                }
                if (rootObj.GetComponent<PlayerSpawner>())
                {
                    Debug.Log("Spawning player");
                    player = Instantiate(playerPrefab, rootObj.transform.position, Quaternion.identity);
                }
                else if (rootObj.GetComponent<EnemySpawner>())
                {
                    Instantiate(enemyTheoPrefab, rootObj.transform.position, Quaternion.identity);
                }
                else if (rootObj.GetComponent<TurretSpawner>())
                {
                    Instantiate(enemyTurretPrefab, rootObj.transform.position, Quaternion.identity);
                }
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
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
