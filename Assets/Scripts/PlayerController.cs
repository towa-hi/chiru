using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
    public bool debugVisualization;
    
    [SerializeField] CharacterController characterController;
    public static PlayerControls input;
    public MeleeWeapon meleeWeapon;
    public Animator handsController;
    public Lighsaber attackEffect;
    
    public float rotationSpeed;
    public float idleMovementSpeed;
    public float attackingMovementSpeed;
    public float stopSpeed;
    public float minRotationDistance;
    
    [SerializeField] Vector3 currentVelocity;
    [SerializeField] Vector3 targetVelocity;
    [SerializeField] Vector2 moveDirection;

    
    [SerializeField] bool attackTriggerSetFlag;
    [SerializeField] bool parryTriggerSetFlag;
    [SerializeField] Vector3 knockbackVector;
    
    void Awake()
    {
        input = new PlayerControls();
        input.CharacterControls.Movement.performed += ctx =>
        {
            moveDirection = ctx.ReadValue<Vector2>();
        };
        input.CharacterControls.Movement.canceled += ctx =>
        {
            moveDirection = Vector2.zero;
        };
    }
    
    void OnEnable()
    {
        input?.CharacterControls.Enable();
    }

    void OnDisable()
    {
        input?.CharacterControls.Disable();
    }

    void Update()
    {
        LookAtCursor(); // try to face the cursor
        HandleAttackInputHeld(); // handle attack inputs
        HandleLeftActionInputHeld(); // handle left click actions
        MovePlayer(); // move
        UpdateAttackEffect(); // do sword trail effect
    }
    
    void LookAtCursor()
    {
        if (!GameManager.ins.cursor)
        {
            return;
        }
        float currentRotationSpeed = meleeWeapon.currentAttackPhase == AttackPhase.IDLE ? rotationSpeed : 0; // set rotation depending on if idle
        Vector3 cursorPos = GameManager.ins.cursor.transform.position;
        cursorPos.y = transform.position.y;
        if (Vector3.Distance(transform.position, cursorPos) > minRotationDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cursorPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.deltaTime);
        }
    }
    
    void HandleAttackInputHeld()
    {
        if (input.CharacterControls.Attack.IsPressed())
        {
            AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
            // idk why this works 
            if (attackTriggerSetFlag)
            {
                if ((currentState.IsName("Swing 1") && currentState.normalizedTime >= 0.3f &&
                     currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 2") && currentState.normalizedTime >= 0.9f &&
                     currentState.normalizedTime < 1.0f))
                {
                    handsController.SetTrigger("AttackTrigger");
                    attackTriggerSetFlag = true;
                }
            }
            else
            {
                if (currentState.IsName("Idle") ||
                    (currentState.IsName("Swing 1") && currentState.normalizedTime >= 0.3f &&
                     currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 2") && currentState.normalizedTime >= 0.9f &&
                     currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 1 Recovery") && currentState.normalizedTime >= 0.1f &&
                     currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 2 Recovery") && currentState.normalizedTime >= 0.1f &&
                     currentState.normalizedTime < 1.0f))
                {
                    handsController.SetTrigger("AttackTrigger");
                    attackTriggerSetFlag = true; 
                }
            }
        }
        else
        {
            attackTriggerSetFlag = false;
        }
    }
    
    void HandleLeftActionInputHeld()
    {
        if (input.CharacterControls.LeftAction.IsPressed())
        {
            if (parryTriggerSetFlag)
            {
                // do nothing because already parrying
            }
            else
            {
                AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
                if (parryTriggerSetFlag && !currentState.IsName("Idle")) return;
                handsController.SetTrigger("ParryTrigger");
                parryTriggerSetFlag = true;
            }
        }
        else
        {
            parryTriggerSetFlag = false;
        }
    }

    void UpdateAttackEffect()
    {
        if (!meleeWeapon) return;
        attackEffect.SetEnabled(meleeWeapon.currentAttackPhase is AttackPhase.ATTACKING or AttackPhase.WINDDOWN);
    }
   


    void MovePlayer()
    {
        // set speed depending on if idle
        float speed = meleeWeapon.currentAttackPhase == AttackPhase.IDLE ? idleMovementSpeed : attackingMovementSpeed;
        float stopThreshold = 0.5f;
        
        // Independent control for X and Z axis
        if (Mathf.Abs(moveDirection.x) > 0)
        {
            targetVelocity.x = moveDirection.x * speed;
        }
        else
        {
            targetVelocity.x = Mathf.Lerp(targetVelocity.x, 0, stopSpeed * Time.deltaTime);
            if (Mathf.Abs(targetVelocity.x) < stopThreshold) targetVelocity.x = 0;
        }

        if (Mathf.Abs(moveDirection.y) > 0)
        {
            targetVelocity.z = moveDirection.y * speed;
        }
        else
        {
            targetVelocity.z = Mathf.Lerp(targetVelocity.z, 0, stopSpeed * Time.deltaTime);
            if (Mathf.Abs(targetVelocity.z) < stopThreshold) targetVelocity.z = 0;
        }

        currentVelocity = targetVelocity;
        if (isKnockedBack)
        {
            currentVelocity *= 0.2f;
            currentVelocity += knockbackVector;
        }
        characterController.Move(currentVelocity * Time.deltaTime);
    }
    
    public override void OnDamaged(float damage, Vector3 damageSourcePosition)
    {
        base.OnDamaged(damage, damageSourcePosition);
        if (!isDead)
        {
            isKnockedBack = true;
            isInvincible = true;
            ChangeMeshColors(Color.red);
            StartCoroutine(KnockbackRoutine(damageSourcePosition));
        }
        Debug.Log("Player took damage");

    }
    
    public float knockbackStrength;
    public float knockbackDuration;
    
    IEnumerator KnockbackRoutine(Vector3 damageSourcePosition)
    {
        Vector3 knockbackDirection = (transform.position - damageSourcePosition).normalized;
        knockbackDirection.y = 0;
        Vector3 startKnockbackVector = knockbackDirection * knockbackStrength;
        Vector3 endKnockbackVector = Vector3.zero;
        
        float timer = 0;
        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;
            knockbackVector = Vector3.Lerp(startKnockbackVector, endKnockbackVector, timer / knockbackDuration);
            yield return null;
        }
        isKnockedBack = false;
        isInvincible = false;
        ResetMeshColors();
        knockbackVector = Vector3.zero;
        Debug.Log("isInvincible false");
    }

    Dictionary<Renderer, Color[]> originalColorsMap = new Dictionary<Renderer, Color[]>();

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
}