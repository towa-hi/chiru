using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    PLAYER,
    ENEMY,
    NEUTRAL
}

// Entity is ALWAYS A ROOT OBJECT
public class Entity : MonoBehaviour
{
    public float maxHp;
    public float hp;
    public List<GameObject> deleteAfterDeath;
    public bool isDead = false;
    public Team team;
    public bool isInvincible;

    public bool isKnockedBack;
    void Awake()
    {
        
    }

    public virtual void OnDamaged(float damage, Vector3 damageSourcePosition)
    {
        if (!isInvincible)
        {
            ApplyDamage(damage);
        }
    }
    

    
    public void ApplyDamage(float damage)
    {
        Debug.Log("applying damage");
        hp -= damage;
        if (hp <= 0)
        {
            OnDeath();
        }
    }

    void OnDeath()
    {
        isDead = true;
        Debug.Log(gameObject.name + " has died");
        // TODO: animation for death
        foreach (var obj in deleteAfterDeath)
        {
            Destroy(obj);
        }
        Destroy(gameObject, 1f);
        enabled = false;
    }
}
