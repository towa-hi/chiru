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
    void Start()
    {
        mainCamera = Camera.main;
        floorLayer = LayerMask.GetMask("Floor");
    }
    void Update()
    {
        MoveToCursor();
    }

    void MoveToCursor()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayer))
        {
            Vector3 worldPoint = hit.point;
            worldPoint.y = 0.01f;
            transform.position = worldPoint;
        }
    }
}
