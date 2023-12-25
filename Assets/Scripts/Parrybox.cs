using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parrybox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (gameObject.activeInHierarchy)
        {
            if (other.CompareTag("Hitbox"))
            {
                Debug.Log("PARRIED!!");
                Entity entity = other.GetComponentInParent<Entity>();
                entity.OnParried();
            }
        }
    }
}
