using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Entity
{
    public bool debugVisualization;
    
    [SerializeField] Cursor cursor;
    [SerializeField] CharacterController characterController;
    public static PlayerControls input;

    public float rotationSpeed = 16.0f;
    public float idleMovementSpeed = 5.0f;
    public float attackingMovementSpeed = 1f;
    public float runSpeed = 10.0f;
    public float stopSpeed = 10f;
    public float minRotationDistance = 0.2f;
    
    [SerializeField] Vector3 currentVelocity;
    [SerializeField] Vector3 targetVelocity;
    [SerializeField] Vector2 moveDirection;
    public MeleeWeapon meleeWeapon;

    public Animator handsController;
    
    public bool attackButtonPressed;

    public Lighsaber attackEffect;
    
    // Start is called before the first frame update
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
        cursor = GameManager.ins.cursor.GetComponent<Cursor>();
    }
    void OnEnable()
    {
        input.CharacterControls.Enable();
    }

    void OnDisable()
    {
        input.CharacterControls.Disable();
    }

    void Update()
    {
        LookAtCursor();
        HandleAttackInputHeld();
        HandleLeftActionInputHeld();
        MovePlayer();
        if (meleeWeapon.currentAttackPhase == AttackPhase.ATTACKING || meleeWeapon.currentAttackPhase == AttackPhase.WINDDOWN)
        {
            attackEffect.SetEnabled(true);
        }
        else
        {
            attackEffect.SetEnabled(false);
        }
    }

    public bool parryTriggerSetFlag;
    void HandleLeftActionInputHeld()
    {
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        bool leftActionButtonPressed = input.CharacterControls.LeftAction.IsPressed();
        if (leftActionButtonPressed)
        {
            if (!parryTriggerSetFlag)
            {
                if (currentState.IsName("Idle"))
                {
                    handsController.SetTrigger("ParryTrigger");
                    parryTriggerSetFlag = true;
                }
            }
        }
        else
        {
            parryTriggerSetFlag = false;
        }
    }

    void SetInput(bool inputEnabled)
    {
        if (inputEnabled)
        {
            input.CharacterControls.Enable();
        }
        else
        {
            input.CharacterControls.Disable();
        }
    }
    
    bool attackTriggerSetFlag;
    void HandleAttackInputHeld()
    {
        // this is disgusting. make it a queue based system ASAP or we wont be able to add more attacks
        attackButtonPressed = input.CharacterControls.Attack.IsPressed();
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        if (attackButtonPressed)
        {
            // idk why this works 
            if (!attackTriggerSetFlag)
            {
                if (currentState.IsName("Idle") ||
                    (currentState.IsName("Swing 1") && currentState.normalizedTime >= 0.3f && currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 2") && currentState.normalizedTime >= 0.9f && currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 1 Recovery") && currentState.normalizedTime >= 0.1f && currentState.normalizedTime < 1.0f) ||
                    (currentState.IsName("Swing 2 Recovery") && currentState.normalizedTime >= 0.1f && currentState.normalizedTime < 1.0f))
                {
                    handsController.SetTrigger("AttackTrigger");
                    attackTriggerSetFlag = true; // Set the flag to true
                }
            }
            else
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
        }
        else
        {
            attackTriggerSetFlag = false; // reset the flag when the attack button is released
        }

        // reset the flag if the state changes
        if (!currentState.IsName("Idle") && !currentState.IsName("Swing 1") && !currentState.IsName("Swing 2"))
        {
            attackTriggerSetFlag = false;
        }
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
        // add forces
        characterController.Move(currentVelocity * Time.deltaTime);
    }

    void LookAtCursor()
    {
        // set rotation depending on if idle
        float currentRotationSpeed = meleeWeapon.currentAttackPhase == AttackPhase.IDLE ? rotationSpeed : 0;
        Vector3 cursorPos = cursor.transform.position;
        cursorPos.y = transform.position.y;
        if (Vector3.Distance(transform.position, cursorPos) > minRotationDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cursorPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gun"))
        {
            Gun gun = other.GetComponent<Gun>();
            if (gun != null)
            {
                gun.SetFireOrigin(this.transform);
                gun.SetFireDestination(cursor.transform);
            }
        }
    }

    public override void OnDamaged(float damage, Vector3 damageSourcePosition)
    {
        base.OnDamaged(damage, damageSourcePosition);
        Debug.Log("Player took damage");
        isInvincible = true;
        SetInput(false);
        StartCoroutine(KnockbackRoutine(damageSourcePosition));
    }
    
    public float knockbackStrength = 1f;
    public float knockbackDuration = 1f;
    bool isKnockedBack = false;
    
    IEnumerator KnockbackRoutine(Vector3 damageSourcePosition)
    {
        Vector3 knockbackDirection = (transform.position - damageSourcePosition).normalized;
        Vector3 potentialKnockbackDestination = transform.position + knockbackDirection * knockbackStrength;

        // Check if the destination is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialKnockbackDestination, out hit, knockbackStrength, NavMesh.AllAreas))
        {
            // Apply knockback if the destination is valid
            float timer = 0;
            while (timer < knockbackDuration)
            {
                timer += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, hit.position, timer / knockbackDuration);
                yield return null;
            }
        }
        else
        {
            // If no valid position is found, don't apply knockback
            Debug.LogWarning("No valid knockback position found on NavMesh.");
        }
        
        SetInput(true);
        isKnockedBack = false;
        isInvincible = false;
        Debug.Log("isInvincible false");
    }
}