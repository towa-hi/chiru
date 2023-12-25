using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 1f;
    public float lifetime = 2f;
    public int damage = 1;
    public Vector3 angle = Vector3.zero;
    
    // Tag of the entity that fired the projectile
    public Entity owner;
    private void OnEnable()
    {
        StartCoroutine(DeactivateAfterLifetime());
    }

    private IEnumerator DeactivateAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        gameObject.SetActive(false);
    }

    public void SetShooterTag(Entity shooter)
    {
        owner = shooter;
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (gameObject.activeInHierarchy)
        {
            //Debug.Log("other: " + other.gameObject + " isactive: " + gameObject.activeSelf);
            if (other.CompareTag("Wall"))
            {
                //Debug.Log("Bullet collided with wall " + other.gameObject);
                Deactivate();
                return;
            }
            Hurtbox hurtbox = other.gameObject.GetComponent<Hurtbox>();
            if (!hurtbox)
            {
                //Debug.Log("Bullet passed through obj " + other.gameObject);
                return;
            }
            // if is shooter
            if (hurtbox.owner == owner)
            {
                //Debug.Log("Bullet passed through self " + hurtbox.owner);
                return;
            }
            // if is same team
            if (hurtbox.owner.team == owner.team)
            {
                //Debug.Log("Bullet collided with friendly " + hurtbox.owner);
                Deactivate();
                return;
            }
            //Debug.Log("Bullet collided with player " + hurtbox.owner);
            hurtbox.owner.OnDamaged(owner.gameObject, damage, owner.transform.position, 0, 0, false);
            Deactivate();
        }
    }

    void Deactivate()
    {
        ObjectPoolManager.ins.ReturnToPool("Projectile", this.gameObject);
        Debug.Log("I died");
        
    }
}
