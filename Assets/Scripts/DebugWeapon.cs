using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWeapon : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    Renderer rend;
    AttackPhase lastAttackPhase;
    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
    }

    void Update()
    {
        if (lastAttackPhase == playerController.currentAttackPhase)
        {
            return;
        }

        rend.material.color = playerController.currentAttackPhase switch
        {
            AttackPhase.IDLE => Color.white,
            AttackPhase.WINDUP => Color.blue,
            AttackPhase.ATTACKING => Color.red,
            AttackPhase.WINDDOWN => Color.magenta,
            _ => throw new ArgumentOutOfRangeException()
        };

        lastAttackPhase = playerController.currentAttackPhase;
    }
}
