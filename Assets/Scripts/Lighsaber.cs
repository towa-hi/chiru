
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighsaber : MonoBehaviour
{
    //The number of vertices to create per frame
    private const int NUM_VERTICES = 12;
    
    [SerializeField]
    [Tooltip("The empty game object located at the tip of the blade")]
    private GameObject _tip = null;

    [SerializeField]
    [Tooltip("The empty game object located at the base of the blade")]
    private GameObject _base = null;

    [SerializeField]
    [Tooltip("The mesh object with the mesh filter and mesh renderer")]
    private GameObject _meshParent = null;

    [SerializeField]
    [Tooltip("The number of frame that the trail should be rendered for")]
    private int _trailFrameLength = 3;

    [SerializeField]
    [ColorUsage(true, true)]
    [Tooltip("The colour of the blade and trail")]
    private Color _colour = Color.red;

    Mesh _mesh;
    Vector3[] _vertices;
    int[] _triangles;
    int _frameCount;
    Vector3 _previousTipPosition;
    Vector3 _previousBasePosition;

    public void SetEnabled(bool enabled)
    {
        _meshParent.SetActive(enabled);
    }
    void Start()
    {
        _meshParent.transform.rotation = Quaternion.identity;
        
        //Init mesh and triangles
        _meshParent.transform.position = Vector3.zero;
        _mesh = new Mesh();
        _meshParent.GetComponent<MeshFilter>().mesh = _mesh;

        Material trailMaterial = Instantiate(_meshParent.GetComponent<MeshRenderer>().sharedMaterial);
        trailMaterial.SetColor("Color_8F0C0815", _colour);
        _meshParent.GetComponent<MeshRenderer>().sharedMaterial = trailMaterial;

        _vertices = new Vector3[_trailFrameLength * NUM_VERTICES];
        _triangles = new int[_vertices.Length];

        //Set starting position for tip and base
        _previousTipPosition = _tip.transform.position;
        _previousBasePosition = _base.transform.position;
        _meshParent.transform.parent = null;
    }
    
    void LateUpdate()
    {
        if(_frameCount == (_trailFrameLength * NUM_VERTICES))
        {
            _frameCount = 0;
        }

        Vector3 tipPosition = _tip.transform.position;
        Vector3 basePosition = _base.transform.position;

        UpdateVertices(tipPosition, basePosition);
        UpdateTriangles();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;

        _previousTipPosition = tipPosition;
        _previousBasePosition = basePosition;
        _frameCount += NUM_VERTICES;
    }

    void UpdateVertices(Vector3 tipPosition, Vector3 basePosition)
    {
        int startIndex = _frameCount % _vertices.Length;
        
        // First triangle vertices for back and front
        _vertices[startIndex] = basePosition;
        _vertices[startIndex + 1] = tipPosition;
        _vertices[startIndex + 2] = _previousTipPosition;
        // Second triangle vertices
        _vertices[startIndex + 3] = basePosition;
        _vertices[startIndex + 4] = _previousTipPosition;
        _vertices[startIndex + 5] = _previousBasePosition;
        // Fill in triangle vertices
        _vertices[startIndex + 6] = _previousTipPosition;
        _vertices[startIndex + 7] = basePosition;
        _vertices[startIndex + 8] = _previousBasePosition;
        
        _vertices[startIndex + 9] = _previousTipPosition;
        _vertices[startIndex + 10] = tipPosition;
        _vertices[startIndex + 11] = basePosition;
    }

    void UpdateTriangles()
    {
        int startIndex = _frameCount % _vertices.Length;

        // Define the triangles
        // Ensure the order of vertices is consistent with the winding order
        for (int i = 0; i < NUM_VERTICES; i += 3)
        {
            _triangles[startIndex + i] = startIndex + i;
            _triangles[startIndex + i + 1] = startIndex + i + 1;
            _triangles[startIndex + i + 2] = startIndex + i + 2;
        }

    }
}
