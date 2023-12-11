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
    public float spreadAngle = 30f;
    public float fireRate = 0.5f;

    private float nextFireTime = 0f;

    void Update()
    {
        
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Shoot()
    {
        Vector3 direction = (fireDestination.position - fireOrigin.position).normalized;
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        
        for (int i = 0; i < numberOfProjectiles; i++)
        {
            float currentAngle = angle - (spreadAngle / 2) + (i * spreadAngle / (numberOfProjectiles - 1));
            //Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);

            Vector3 bulletDirection = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(currentAngle * Mathf.Deg2Rad));
            Quaternion bulletRotation = Quaternion.LookRotation(bulletDirection, Vector3.up);

            Vector3 spawnPosition = fireOrigin.position;
            //spawnPosition.y = 0f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, bulletRotation);
            string parent = gameObject.transform.parent.tag;
            Debug.Log("before setting shooter tag the object is: " + parent);
            projectile.GetComponent<Projectile>().SetShooterTag(parent);
        }

    }

}
