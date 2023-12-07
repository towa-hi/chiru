using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    Camera mainCamera;
    Vector2 mousePosition;
    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        MoveToCursor();
    }

    void MoveToCursor()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 worldpoint = hit.point;
            worldpoint.y = 0.01f;
            transform.position = worldpoint;
        }
    }
}
