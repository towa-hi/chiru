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
    public bool isParrying;
    public bool isReceivingRiposte;
    public float parryDuration;
    public List<Rigidbody> ragdollParts;
    public GameObject model;
    public GameObject ragdoll;
    
    Coroutine knockbackRoutine;
    Coroutine parryCoroutine;
    public virtual void OnDamaged(GameObject source, float damage, Vector3 damageSourcePosition, float knockbackMagnitude, float knockbackDuration, bool applyInvincibility)
    {
        if (isInvincible) return;
        ApplyDamage(damage, damageSourcePosition);
        if (isDead) return;
        if (knockbackMagnitude > 0 && knockbackDuration > 0)
        {
            knockbackRoutine = StartCoroutine(KnockbackRoutine(damageSourcePosition, knockbackMagnitude, knockbackDuration, applyInvincibility));
        }
    }
    
    public void ApplyDamage(float damage, Vector3 damageSourcePosition)
    {
        Debug.Log("applying damage");
        hp -= damage;
        if (hp <= 0)
        {
            OnDeath(damageSourcePosition);
        }
    }

    void OnDeath(Vector3 damageSourcePosition)
    {
        isDead = true;
        StopAllCoroutines();
        Debug.Log(gameObject.name + " has died");
        // TODO: animation for death
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.enabled = false;
        }
        foreach (GameObject obj in deleteAfterDeath)
        {
            Destroy(obj);
        }
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }
        if (parryCoroutine != null)
        {
            StopCoroutine(parryCoroutine);
        }

        if (model && ragdoll)
        {
            model.SetActive(false);
            ragdoll.SetActive(true);
            Vector3 knockbackDirection = (transform.position - damageSourcePosition).normalized;
            knockbackDirection.y = 0;
            float force = 100f;
            foreach (var rb in ragdoll.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
                rb.AddForce(knockbackDirection * force);
            }
        }

        Destroy(gameObject, 5f);
    }
    
    IEnumerator KnockbackRoutine(Vector3 damageSourcePosition, float knockbackMagnitude, float knockbackDuration, bool applyInvincibility)
    {
        // before knockback
        isKnockedBack = true;
        isInvincible = applyInvincibility;
        Vector3 knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        Vector3 startKnockbackVector = knockbackDirection * knockbackMagnitude;
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.SetTrigger("IsStunned");
        }
        ChangeMeshColors(Color.red);
        float knockbackTimer = 0f;
        // while knockback
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
    
    void LerpMeshColorsBack(float lerpValue)
    {
        foreach (var kvp in originalColorsMap)
        {
            Renderer renderer = kvp.Key;
            Color[] originalColors = kvp.Value;
            for (int i = 0; i < originalColors.Length; i++)
            {
                Material mat = renderer.materials[i];
                if (mat && mat.HasProperty("_Color"))
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
                if (mat && mat.HasProperty("_Color"))
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
                if (mat && mat.HasProperty("_Color"))
                {
                    // Restore the original color
                    mat.color = originalColors[i];
                }
            }
        }
        originalColorsMap.Clear();
    }

    public void OnParried()
    {
        SoundPlayer soundPlayer = GetComponent<SoundPlayer>();
        if (soundPlayer)
        {
            soundPlayer.PlayWeaponSound("swordClash");
        }
        parryCoroutine = StartCoroutine(ResetParryEffect());
    }
    
    IEnumerator ResetParryEffect()
    {
        // before parryable
        isParried = true;
        Animator animator = GetComponent<Animator>();
        if (animator)
        {
            animator.SetTrigger("IsStunned");
            Debug.Log("IsStunned true");
        }
        // while parryable
        yield return new WaitForSeconds(parryDuration);
        isParried = false;
        if (animator)
        {
            animator.SetTrigger("IsUnStunned");
            Debug.Log("IsStunned true");
        }
        // after parry
        isParried = false;
        Debug.Log("Parry effect ended");
    }

    public void OnReceivingRiposte()
    {
        isReceivingRiposte = true;
        StopCoroutine(parryCoroutine);
        // isParried remains true
    }

    public void OnReceivingRiposteEnd()
    {
        isReceivingRiposte = false;
        isParried = false;
        ApplyDamage(1000, GameManager.ins.player.transform.position);
    }
}
