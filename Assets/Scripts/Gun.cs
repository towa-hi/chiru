using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform fireOrigin;
    public Transform fireDestination;
    public int numberOfProjectiles = 1;
    public float scatterAngle = 30f;
    public float deviationAngle = 20f;
    public float fireRate = 5f;
    public ObjectPoolManager objectPoolManager;
    private float nextFireTime = 0f;

    private string parent;

    public void OnPickup()
    {
        Debug.Log("gun picked up");
        parent = gameObject.transform.parent.tag;
        // Implement specific pickup behavior for the gun
        // For example, add it to the player's inventory
    }
    
    public void SetFireOrigin(Transform origin)
    {
        fireOrigin = origin;
    }

    public void SetFireDestination(Transform destination)
    {
        fireDestination = destination;
    }

    void Update()
    {
        if (fireOrigin != null && fireDestination != null)
        {
            if (parent == "Player")
            {
                if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }
    
    }

    void Shoot()
    {
        Vector3 direction = (fireDestination.position - fireOrigin.position).normalized;
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float deviation = Random.Range(-deviationAngle, deviationAngle);;

            float currentAngle;
            if (numberOfProjectiles > 1)
            {
                currentAngle = angle - (scatterAngle / 2) + (i * scatterAngle / (numberOfProjectiles - 1)) + deviation;
            }
            else
            {
                currentAngle = angle + deviation;
            }

            Vector3 bulletDirection = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(currentAngle * Mathf.Deg2Rad));
            Quaternion bulletRotation = Quaternion.LookRotation(bulletDirection, Vector3.up);

            Vector3 spawnPosition = fireOrigin.position;
            GameObject projectile = objectPoolManager.GetFromPool("Projectile", spawnPosition, bulletRotation);
            projectile.SetActive(true);
            parent = gameObject.transform.parent.tag;
            Debug.Log("before setting shooter tag the object is: " + parent);
            projectile.tag = "Projectile";
            projectile.GetComponent<Projectile>().SetShooterTag(parent);
        }

    }

}
