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
    public Vector3 knockbackVector;
    Dictionary<Renderer, Color[]> originalColorsMap = new Dictionary<Renderer, Color[]>();
    public bool isParried;

    Coroutine knockbackRoutine;
    
    public virtual void OnDamaged(GameObject source, float damage, Vector3 damageSourcePosition, float knockbackMagnitude, float knockbackDuration, bool applyInvincibility)
    {
        if (!isInvincible)
        {
            ApplyDamage(damage);
            if (!isDead)
            {
                if (knockbackMagnitude == 0 || knockbackDuration == 0)
                {
                    return;
                }

                if (knockbackRoutine != null)
                {
                    StopCoroutine(knockbackRoutine);
                }
                knockbackRoutine = StartCoroutine(KnockbackRoutine(damageSourcePosition, knockbackMagnitude, knockbackDuration,
                    applyInvincibility));
            }
        }
    }

    public void SetKnockback()
    {
        
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
    
    IEnumerator KnockbackRoutine(Vector3 damageSourcePosition, float knockbackMagnitude, float knockbackDuration, bool applyInvincibility)
    {
        isKnockedBack = true;
        Debug.Log("KnockbackRoutine started");
        Vector3 knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        Vector3 startKnockbackVector = knockbackDirection * knockbackMagnitude;
        if (applyInvincibility)
        {
            isInvincible = true;
        }
        Animator animator = GetComponent<Animator>();
        Debug.Log("Knockback started");
        if (animator)
        {
            animator.SetTrigger("IsStunned");
            Debug.Log("IsStunned true");
        }
        ChangeMeshColors(Color.red);
        float knockbackTimer = 0f;
        while (knockbackTimer < knockbackDuration)
        {
            knockbackTimer += Time.deltaTime;
            knockbackVector = Vector3.Lerp(startKnockbackVector, Vector3.zero, knockbackTimer / knockbackDuration);
            LerpMeshColorsBack(knockbackTimer / knockbackDuration);
            yield return null;
        }
        // after knockback
        ResetMeshColors();
        knockbackVector = Vector3.zero;
        isKnockedBack = false;
        isInvincible = false;
        knockbackRoutine = null;
        if (animator)
        {
            animator.SetTrigger("IsUnStunned");
            Debug.Log("IsStunned true");
        }
        Debug.Log("Knockback ended");
    }
    
    void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        Debug.Log(gameObject.name + " has died");
        // TODO: animation for death
        foreach (GameObject obj in deleteAfterDeath)
        {
            Destroy(obj);
        }
        Destroy(gameObject, 1f);
        enabled = false;
    }

    void LerpMeshColorsBack(float lerpValue)
    {
        foreach (var kvp in originalColorsMap)
        {
            Renderer renderer = kvp.Key;
            Color[] originalColors = kvp.Value;
            for (int i = 0; i < originalColors.Length; i++)
            {
                Material mat = renderer.materials[i];
                if (mat != null && mat.HasProperty("_Color"))
                {
                    mat.color = Color.Lerp(Color.red, originalColors[i], lerpValue);
                }
            }
        }
    }
    
    void ChangeMeshColors(Color color)
    {
        originalColorsMap.Clear();
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            Color[] originalColors = new Color[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                Material mat = renderer.materials[i];
                if (mat.HasProperty("_Color"))
                {
                    // Store the original color
                    originalColors[i] = mat.color;
                    // Change to the new color
                    mat.color = color;
                }
            }
            originalColorsMap.Add(renderer, originalColors);
        }
    }

    void ResetMeshColors()
    {
        foreach (var kvp in originalColorsMap)
        {
            Renderer renderer = kvp.Key;
            Color[] originalColors = kvp.Value;
            for (int i = 0; i < originalColors.Length; i++)
            {
                Material mat = renderer.materials[i];
                if (mat != null && mat.HasProperty("_Color"))
                {
                    // Restore the original color
                    mat.color = originalColors[i];
                }
            }
        }
        originalColorsMap.Clear();
    }

    public float parryDuration;
    
    public void OnParried()
    {
        Debug.Log("I got parried!");
        

        Animator animator = GetComponent<Animator>();
        SoundPlayer soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer)
        {
            soundPlayer.PlaySound("parry");
            soundPlayer.PlayWeaponSound("swordClash");
        }
        StartCoroutine(ResetParryEffect());
    }
    
    IEnumerator ResetParryEffect()
    {
        Animator animator = GetComponent<Animator>();
        isParried = true;
        if (animator)
        {
            animator.SetTrigger("IsStunned");
            Debug.Log("IsStunned true");
        }
        yield return new WaitForSeconds(parryDuration);
        isParried = false;
        if (animator)
        {
            animator.SetTrigger("IsUnStunned");
            Debug.Log("IsStunned true");
        }

        isParried = false;
        Debug.Log("Parry effect ended");
    }
    
}
