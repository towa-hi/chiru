using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuiPlayerHealthBar : MonoBehaviour
{
    public float fullHealthWidth = 190f; // Full width of the health bar at max health
    public Image healthBar;
    public float maxHp;
    float currentHp = 0;

    void Update()
    {
        if (GameManager.ins.player)
        {
            PlayerController player = GameManager.ins.player.GetComponent<PlayerController>();
            maxHp = player.maxHp;
            currentHp = player.hp;

            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        if (maxHp > 0)
        {
            float healthPercentage = currentHp / maxHp;
            // Hide health bar if health percentage is less than or equal to zero
            if (healthPercentage <= 0)
            {
                healthBar.gameObject.SetActive(false);
            }
            else
            {
                healthBar.gameObject.SetActive(true);
                healthBar.rectTransform.sizeDelta = new Vector2(fullHealthWidth * healthPercentage, healthBar.rectTransform.sizeDelta.y);
            }
        }
    }
}