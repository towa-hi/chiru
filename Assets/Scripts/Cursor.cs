using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    Camera mainCamera;
    Vector3 mousePosition;
    Vector3 lastMousePos;
    LayerMask floorLayer;
    public float edgeBuffer;
    void Start()
    {
        mainCamera = Camera.main;
        floorLayer = LayerMask.GetMask("Floor");
        
    }
    void FixedUpdate()
    {
        MoveToCursor();
    }

    void MoveToCursor()
    {
        //Vector2 mousePos = PlayerController.input.CharacterControls.MouseControl.ReadValue<Vector2>();
        //Vector2 mouseVel = PlayerController.input.CharacterControls.MouseControl.ReadValue<Vector2>();
        Vector3 mouseDelta = GetMouseDelta();
        TeleportMouse();
        mousePosition += mouseDelta;
        Debug.Log(mousePosition);
        //mousePosition.x = Mathf.Clamp(mousePosition.x, edgeBuffer, Screen.width - edgeBuffer);
        //mousePosition.y = Mathf.Clamp(mousePosition.y, edgeBuffer, Screen.height - edgeBuffer);
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayer))
        {
            Vector3 worldPoint = hit.point;
            worldPoint.y = 0.01f;
            transform.position = worldPoint;
        }
    }

    public Vector3 GetMouseDelta()
    {
        Vector3 currentPos = PlayerController.input.CharacterControls.MouseControl.ReadValue<Vector2>();
        if (lastMousePos == Vector3.zero)
        {
            lastMousePos = currentPos;
        }

        Vector3 mouseDelta = currentPos - lastMousePos;
        lastMousePos = currentPos;
        return mouseDelta;

    }

    void TeleportMouse()
    {
        Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));
        if (mousePosition.x > Screen.width)
        {
            //lastMousePos = Vector3.zero;
        }
        
    }
}
