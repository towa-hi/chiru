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
    [SerializeField] float movementSmoothingTime = 0.1f;
    Vector3 currentVelocity;
    Vector3 targetVelocity;
    [SerializeField] Vector2 moveDirection;
    bool movementPressed;
    
    
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
        targetVelocity = new Vector3(moveDirection.x, 0, moveDirection.y) * movementSpeed;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocity, movementSmoothingTime);
        characterController.Move(currentVelocity * Time.fixedDeltaTime);
    }
    void LookAtCursor()
    {
        Vector3 cursorPos = cursor.transform.position;
        cursorPos.y = transform.position.y;

        Quaternion targetRotation = Quaternion.LookRotation(cursorPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }
}
