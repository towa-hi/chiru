using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GuiController : MonoBehaviour
{
    public GameObject deathPanel;
    public GameObject winPanel;
    public GameObject healthBar;
    public Button restartButton;

    private InputAction restartAction; // Input action for restarting

    void Awake()
    {
        // Initialize the restart action
        restartAction = new InputAction("Restart", binding: "<Keyboard>/space");
        restartAction.performed += _ => OnRestartButton(); // Subscribe to the action's performed event
    }

    void OnEnable()
    {
        restartAction.Enable(); // Enable the action
    }

    void OnDisable()
    {
        restartAction.Disable(); // Disable the action
    }

    public void OnDeath()
    {
        deathPanel.SetActive(true);
    }

    public void OnWin()
    {
        winPanel.SetActive(true);
    }
    
    public void OnRestartButton()
    {
        Debug.Log("space pressed");
        // if (deathPanel.activeInHierarchy)
        // {
        //     GameManager.ins.OnRestart();
        //     deathPanel.SetActive(false);
        // }
    }
}