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
        SceneManager.LoadScene("GameScene");
    }
    public void ContinueSavedGame()
    {
        
    }

    public void SaveGame()
    {
        
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
