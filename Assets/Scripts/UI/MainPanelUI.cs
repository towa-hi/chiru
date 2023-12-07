using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanelUI : MonoBehaviour
{
    [SerializeField] MenuButtonUI continueButton;
    [SerializeField] MenuButtonUI newGameButton;
    [SerializeField] MenuButtonUI optionsButton;
    [SerializeField] MenuButtonUI quitButton;
    [SerializeField] GameObject continuePanel;

    void Start()
    {
        continueButton.button.onClick.AddListener(OnContinueButton);
        newGameButton.button.onClick.AddListener(OnNewGameButton);
        optionsButton.button.onClick.AddListener(OnOptionsButton);
        quitButton.button.onClick.AddListener(OnQuitButton);
        
    }

    void OnContinueButton()
    {
        
    }

    void OnNewGameButton()
    {
        GameManager.ins.NewGame();
    }

    void OnOptionsButton()
    {
        GameManager.ins.menuUI.OpenOptions();
    }

    void OnQuitButton()
    {
        Debug.Log("Yeet");
        GameManager.ins.QuitGame();
    }
}
