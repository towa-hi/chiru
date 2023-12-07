using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsPanelUI : MonoBehaviour
{
    [SerializeField] MenuButtonUI returnButton;

    void Start()
    {
        returnButton.button.onClick.AddListener(OnReturnButton);
    }

    void OnReturnButton()
    {
        GameManager.ins.menuUI.OpenMain();
    }
}
