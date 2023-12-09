using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    Camera mainCamera;
    Vector2 mousePosition;
    LayerMask floorLayer;
    Vector3 mousePos;
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
        Vector3 mousePos = Mouse.current.position.ReadValue();
        //Vector3 mouseVel = Mouse.current.delta.ReadValue();
        //mousePos += mouseVel;
        mousePos.x = Mathf.Clamp(mousePos.x, edgeBuffer, Screen.width - edgeBuffer);
        mousePos.y = Mathf.Clamp(mousePos.y, edgeBuffer, Screen.height - edgeBuffer);
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayer))
        {
            Vector3 worldPoint = hit.point;
            worldPoint.y = 0.01f;
            transform.position = worldPoint;
        }
    }
}
