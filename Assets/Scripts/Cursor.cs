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
    public float mouseSensitivity;
    
    void Start()
    {
        mainCamera = Camera.main;
        floorLayer = LayerMask.GetMask("Floor");
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        MoveToCursor();
    }

    void MoveToCursor()
    {
        // Read the mouse delta
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Accumulate the delta to the mousePosition
        mousePosition += new Vector3(mouseDelta.x, mouseDelta.y, 0) * mouseSensitivity;

        // Clamp the mousePosition to the screen bounds minus edgeBuffer
        mousePosition.x = Mathf.Clamp(mousePosition.x, edgeBuffer, Screen.width - edgeBuffer);
        mousePosition.y = Mathf.Clamp(mousePosition.y, edgeBuffer, Screen.height - edgeBuffer);

        // Convert the accumulated position to a world point
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayer))
        {
            Vector3 worldPoint = hit.point;
            worldPoint.y = 0.01f; // Adjust Y position as needed
            transform.position = worldPoint;
        }
    }
}
