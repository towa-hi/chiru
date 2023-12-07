using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    GameObject activePanel;
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject optionsPanel;
    List<GameObject> allPanels;
    
    public void Activate(bool isActive)
    {
        if (isActive)
        {
            gameObject.SetActive(true);
            activePanel = null;
            allPanels = new List<GameObject>
            {
                mainPanel,
                optionsPanel
            };
            SetActivePanel(mainPanel);
        }
        else
        {
            gameObject.SetActive(false);
            activePanel = null;
        }
    }

    public void OpenMain()
    {
        SetActivePanel(mainPanel);
    }
    
    public void OpenOptions()
    {
        SetActivePanel(optionsPanel);
    }
    
    void SetActivePanel(GameObject newPanel)
    {
        if (newPanel != activePanel)
        {
            foreach (GameObject panel in allPanels)
            {
                panel.SetActive(newPanel == panel);
            }
        }
    }
    
    
}
