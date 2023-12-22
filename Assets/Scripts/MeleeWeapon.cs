using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public Entity owner;
    public bool debugVisualization;
    
    public float damage;
    public AttackPhase currentAttackPhase;
    AttackPhase lastAttackPhase;
    [SerializeField] Renderer debugRenderer;
    
    
    void Awake()
    {
        owner = GetComponentInParent<Entity>();
        lastAttackPhase = AttackPhase.IDLE;
        currentAttackPhase = AttackPhase.IDLE;
    }

    void Update()
    {
        if (debugVisualization)
        {
            if (lastAttackPhase != currentAttackPhase)
            {
                debugRenderer.material.color = currentAttackPhase switch
                {
                    AttackPhase.IDLE => Color.white,
                    AttackPhase.WINDUP => Color.blue,
                    AttackPhase.ATTACKING => Color.red,
                    AttackPhase.WINDDOWN => Color.magenta,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

        }
        // do at end
        lastAttackPhase = currentAttackPhase;
    }

    void OnTriggerEnter(Collider other)
    {
        // if not attacking
        if (currentAttackPhase != AttackPhase.ATTACKING)
        {
            return;
        }
        // if dead
        if (owner.isDead)
        {
            return;
        }
        Hurtbox hurtbox = other.gameObject.GetComponent<Hurtbox>();
        // if collider is not a hurtbox
        if (!hurtbox)
        {
            return;
        }
        // if friendly
        if (hurtbox.owner.team == owner.team)
        {
            return;
        }
        // do damage
        hurtbox.owner.OnDamaged(damage, owner.transform.position);
    }
}

public enum AttackPhase
{
    IDLE,
    WINDUP,
    ATTACKING,
    WINDDOWN
}