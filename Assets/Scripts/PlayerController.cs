using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController ins;
    
    [SerializeField] Cursor cursor;

    [SerializeField] CharacterController characterController;
    public static PlayerControls input;

    [SerializeField] float rotationSpeed = 16.0f;
    [SerializeField] float walkSpeed = 5.0f;
    [SerializeField] float runSpeed = 10.0f;
    [SerializeField] float stopSpeed = 10f;
    [SerializeField] float minRotationDistance = 0.2f;
    
    [SerializeField] Vector3 currentVelocity;
    [SerializeField] Vector3 targetVelocity;
    [SerializeField] Vector2 moveDirection;
    bool movementPressed;
    bool isRunning;
    
    public bool isAttacking;

    public AttackPhase currentAttackPhase = AttackPhase.IDLE;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (ins != null && ins != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            ins = this;
        }
        input = new PlayerControls();
        input.CharacterControls.Movement.performed += ctx =>
        {
            moveDirection = ctx.ReadValue<Vector2>();
            movementPressed = moveDirection.x != 0 || moveDirection.y != 0;
        };

        input.CharacterControls.Movement.canceled += ctx =>
        {
            moveDirection = Vector2.zero;
            movementPressed = false;
        };
        input.CharacterControls.Run.performed += ctx =>
        {
            isRunning = true;
        };
        input.CharacterControls.Run.canceled += ctx =>
        {
            isRunning = false;
        };
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
        MovePlayer();
    }

    void MovePlayer()
    {
        float speed;
        if (currentAttackPhase == AttackPhase.IDLE)
        {
            speed = isRunning ? runSpeed : walkSpeed;
        }
        else
        {
            speed = walkSpeed * 0.2f;
        }

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
        characterController.Move(currentVelocity * Time.deltaTime);
    }

    void LookAtCursor()
    {
        float currentRotationSpeed = currentAttackPhase == AttackPhase.IDLE ? rotationSpeed : 0;
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
}
public enum AttackPhase
{
    IDLE,
    WINDUP,
    ATTACKING,
    WINDDOWN
}