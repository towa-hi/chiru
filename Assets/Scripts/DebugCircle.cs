using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DebugCircle : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float radius = 1.0f;
    public int segments = 36;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        DrawCircle();
    }

    void OnValidate()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        DrawCircle();
    }

    void Update()
    {
        DrawCircle();
    }

    void DrawCircle()
    {
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;

        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), 0f, radius * Mathf.Sin(theta));
            lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }
}
