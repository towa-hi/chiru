using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DebugLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float length = 1.0f;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        DrawFacingDirection();
    }
    
    void OnValidate()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        DrawFacingDirection();
    }

    void Update()
    {
        DrawFacingDirection();
    }

    void DrawFacingDirection()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + transform.forward * length;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);
    }
}
