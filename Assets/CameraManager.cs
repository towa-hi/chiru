using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Vector3 verticalOffset;
    public float maxForwardOffset;
    public float lerpSpeed;
    public float zoomLerpSpeed;
    public float zoomInSize;
    public float normalSize;

    Camera cam;
    float shakeTimeRemaining;

    void Start()
    {
        cam = GetComponent<Camera>();
    }
    void Update()
    {
        if (GameManager.ins.player && GameManager.ins.cursor)
        {
            Vector3 newFocus = FindSweetSpot();
            transform.position = Vector3.Lerp(transform.position, newFocus + verticalOffset, lerpSpeed * Time.deltaTime);

            if (GameManager.ins.player.GetComponent<PlayerController>().isRiposte)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomInSize, zoomLerpSpeed * Time.deltaTime);
            }
            else
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, normalSize, zoomLerpSpeed * Time.deltaTime);
            }
        }
    }

    Vector3 FindSweetSpot()
    {
        if (GameManager.ins.player && GameManager.ins.cursor)
        {
            Vector3 cursorPos = GameManager.ins.cursor.transform.position;
            Vector3 playerPos = GameManager.ins.player.transform.position;
            return Vector3.Lerp(playerPos, cursorPos, maxForwardOffset);
        }
        else
        {
            return Vector3.zero;
        }
    }
}
