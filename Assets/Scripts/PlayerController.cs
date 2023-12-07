using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Cursor cursor;

    [SerializeField] CharacterController characterController;
    PlayerControls input;

    [SerializeField] float rotationSpeed = 16.0f;
    [SerializeField] float movementSpeed = 5.0f;
    [SerializeField] float walkSpeed = 5.0f;
    [SerializeField] float runSpeed = 10.0f;
    [SerializeField] float movementSmoothingTime = 0.1f;
    [SerializeField] float minRotationDistance = 0.2f;
    
    [SerializeField] Vector3 currentVelocity;
    [SerializeField] Vector3 targetVelocity;
    [SerializeField] Vector2 moveDirection;
    bool movementPressed;
    bool isRunning;
    
    // Start is called before the first frame update
    void Awake()
    {
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

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        LookAtCursor();
        MovePlayer();
    }

    void MovePlayer()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        targetVelocity = new Vector3(moveDirection.x, 0, moveDirection.y) * speed;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocity, movementSmoothingTime);
        characterController.Move(currentVelocity * Time.fixedDeltaTime);
    }
    void LookAtCursor()
    {
        Vector3 cursorPos = cursor.transform.position;
        cursorPos.y = transform.position.y;
        if (Vector3.Distance(transform.position, cursorPos) > minRotationDistance)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cursorPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
