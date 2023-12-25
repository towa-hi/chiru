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

        if (isRiposte)
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

    float riposteDistance = 50f;
    bool isRiposte;
    Entity targetForRiposte;
    void HandleAttackInputHeld()
    {
        if (input.CharacterControls.Attack.IsPressed())
        {
            if (isRiposte)
            {
                return;
            }
            Entity closestParriedEntity = FindClosestParriedEntity();
            if (closestParriedEntity)
            {
                PerformRiposte(closestParriedEntity);
                return;
            }
            
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

    Entity FindClosestParriedEntity()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, riposteDistance);
        Entity closestParriedEntity = null;
        float closestDistance = float.MaxValue;
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("EnemyHurtbox"))
            {
                Entity entity = collider.GetComponentInParent<Entity>();
                if (entity.isParried)
                {
                    Vector3 directionToEntity = (entity.transform.position - transform.position).normalized;
                    float angleToEntity = Vector3.Angle(transform.forward, directionToEntity);
                    float distanceToEntity = Vector3.Distance(transform.position, entity.transform.position);
                    if (distanceToEntity < closestDistance && angleToEntity < 45) // 45 degrees as an example
                    {
                        closestParriedEntity = entity;
                        closestDistance = distanceToEntity;
                    }
                }
            }
        }
        return closestParriedEntity;
    }

    void PerformRiposte(Entity target)
    {
        isRiposte = true;
        targetForRiposte = target;
        Vector3 enemyPosition = targetForRiposte.transform.position;
        Vector3 directionToEnemy = (enemyPosition - transform.position).normalized;
        Vector3 newPosition = enemyPosition - directionToEnemy * 1.0f; // Adjust the multiplier as needed
        newPosition.y = transform.position.y; // Keep the player's original y position to avoid moving up or down

        // Calculate the movement vector needed to reach the new position
        Vector3 moveVector = newPosition - transform.position;

        // Move the player to the new position
        characterController.Move(moveVector);

        transform.LookAt(new Vector3(enemyPosition.x, transform.position.y, enemyPosition.z));
        handsController.Play("Riposte");
        Debug.Log("Riposte started");
    }

    public void OnRiposteEnd()
    {
        Debug.Log("Riposte ended");
        isRiposte = false;
        targetForRiposte = null;
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
        if (isRiposte)
        {
            return;
        }
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
        // add knockback force
        if (isKnockedBack)
        {
            currentVelocity *= 0.2f;
            currentVelocity += knockbackVector;
        }
        characterController.Move(currentVelocity * Time.deltaTime);
    }

    public void OnParry()
    {
        
    }
    
    public override void OnDamaged(GameObject source, float damage, Vector3 damageSourcePosition, float knockbackMagnitude, float knockbackDuration, bool applyInvincibility)
    {
        base.OnDamaged(source, damage, damageSourcePosition, knockbackMagnitude, knockbackDuration, applyInvincibility);
        Debug.Log("Player took damage");

    }
}