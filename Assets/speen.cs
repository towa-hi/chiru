using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speen : MonoBehaviour
{
    public float rotationSpeed = 50.0f; // Rotation speed in degrees per second

    // Update is called once per frame
    void Update()
    {
        // Rotate around the Y-axis
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
