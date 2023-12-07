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
    void FixedUpdate()
    {

        Vector3 forwardOffset = player.transform.forward * maxForwardOffset;
        Vector3 newFocus = player.transform.position + forwardOffset;
        transform.position = Vector3.Lerp(transform.position, newFocus + verticalOffset, lerpSpeed * Time.fixedDeltaTime);
    }
}
