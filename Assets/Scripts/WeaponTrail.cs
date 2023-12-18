using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{

    [SerializeField] GameObject tip;
    [SerializeField] GameObject hilt;

    [SerializeField] GameObject trailMesh;

    [SerializeField] int trailFrameLength;

    Mesh mesh;

    [SerializeField] Vector3[] vertices;

    [SerializeField] int[] triangles;

    int frameCount;

    [SerializeField] Vector3 previousTipPosition;

    [SerializeField] Vector3 previousHiltPosition;

    const int NUM_VERTICES = 12;
    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        trailMesh.GetComponent<MeshFilter>().mesh = mesh;
        
        vertices = new Vector3[trailFrameLength * NUM_VERTICES];
        triangles = new int[vertices.Length];

        previousTipPosition = tip.transform.position;
        previousHiltPosition = hilt.transform.position;
    }

    void Update()
    {
        if (frameCount == trailFrameLength * NUM_VERTICES)
        {
            frameCount = 0;
        }

        vertices[frameCount] = hilt.transform.position;
        vertices[frameCount + 1] = tip.transform.position;
        vertices[frameCount + 2] = previousTipPosition;

        vertices[frameCount + 3] = hilt.transform.position;
        vertices[frameCount + 4] = previousTipPosition;
        vertices[frameCount + 5] = tip.transform.position;

        vertices[frameCount + 6] = previousTipPosition;
        vertices[frameCount + 7] = hilt.transform.position;
        vertices[frameCount + 8] = previousHiltPosition;

        vertices[frameCount + 9] = previousTipPosition;
        vertices[frameCount + 10] = previousHiltPosition;
        vertices[frameCount + 11] = hilt.transform.position;

        triangles[frameCount] = frameCount;
        triangles[frameCount + 1] = frameCount + 1;
        triangles[frameCount + 2] = frameCount + 2;
        triangles[frameCount + 3] = frameCount + 3;
        triangles[frameCount + 4] = frameCount + 4;
        triangles[frameCount + 5] = frameCount + 5;
        triangles[frameCount + 6] = frameCount + 6;
        triangles[frameCount + 7] = frameCount + 7;
        triangles[frameCount + 8] = frameCount + 8;
        triangles[frameCount + 9] = frameCount + 9;
        triangles[frameCount + 10] = frameCount + 10;
        triangles[frameCount + 11] = frameCount + 11;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        previousTipPosition = tip.transform.position;
        previousHiltPosition = hilt.transform.position;
    }
}
