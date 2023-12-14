using UnityEngine;

public class Item : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public float floatAmplitude = 0.5f;
    public float floatFrequency = 1f;

    private Vector3 startPos;
    private Vector3 tempPos;

    protected virtual void Start()
    {
        startPos = transform.position;
    }

    protected virtual void Update()
    {
        RotateAndFloat();
    }
    
    protected void RotateAndFloat()
    {
        // Rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Floating
        tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * floatFrequency) * floatAmplitude;

        transform.position = tempPos;
    }

    public virtual void OnPickup()
    {
        // Define what happens when the item is picked up
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Make sure the player has the tag "Player"
        {
            OnPickup();
        }
    }

}