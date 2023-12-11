using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    public int damage = 1;
    public Vector3 angle = Vector3.zero;
    
    // Tag of the entity that fired the projectile
    private string shooterTag;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetShooterTag(string tag)
    {
        shooterTag = tag;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Floor")
        {
            return;
        }
        Debug.Log(other.gameObject.name);
        Debug.Log("shooter is " + shooterTag + " and its collided with " + other.gameObject.tag);
        bool collided = false;
        switch (other.gameObject.tag)
        {
            case "Enemy":
                if (shooterTag != "Enemy")
                {
                    //collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);
                    collided = true;
                }
                break;

            case "Player":
                if (shooterTag != "Player")
                {
                    //collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);
                    collided = true;
                }
                break;
            
            case "Projectile":
                break;
            
            case "Wall":
            default:
                collided = true;
                break;
        }

        if (collided)
        {
            Debug.Log("I have collided with " + other.gameObject.tag + "now I will die");
            Destroy(gameObject);
        }
    }


}
