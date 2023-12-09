using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] Cursor cursor;
    
    public Vector3 verticalOffset;
    public float maxForwardOffset;
    public float lerpSpeed;
    
    void Update()
    {
        
        Vector3 newFocus = FindSweetSpot();
        transform.position = Vector3.Lerp(transform.position, newFocus + verticalOffset, lerpSpeed * Time.deltaTime);
    }

    Vector3 FindSweetSpot()
    {
        Vector3 cursorPos = cursor.transform.position;
        Vector3 playerPos = player.transform.position;
        return Vector3.Lerp(playerPos, cursorPos, maxForwardOffset);
    }
}
