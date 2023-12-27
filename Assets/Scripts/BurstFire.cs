using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFire : MonoBehaviour
{
    public Entity owner;
    public GameObject projectilePrefab;
    public Transform firePoint;

    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.1f;
    public float timeBetweenBursts = 1.0f;

    private bool isFiring = false;

    public void StartFiring()
    {
        if (isFiring)
        {
            return;
        }
        StartCoroutine(FireBurst());
    }
    
    IEnumerator FireBurst()
    {
        isFiring = true;
        SoundPlayer soundPlayer = GetComponent<SoundPlayer>();
        soundPlayer.PlaySound("gunshot");
        for (int i = 0; i < shotsPerBurst; i++)
        {
            if (owner != null && owner.isDead)
            {
                isFiring = false;
                yield break;
            }
            FireProjectile();
            yield return new WaitForSeconds(timeBetweenShots);
        }
        
        yield return new WaitForSeconds(timeBetweenBursts);
        isFiring = false;
    }

    void FireProjectile()
    {
        // Calculate a random deviation
        float deviationAngle = Random.Range(-15.0f, 15.0f); // Adjust the range as needed
        Quaternion deviation = Quaternion.Euler(0, deviationAngle, 0);

        // Apply the deviation to the projectile's rotation
        Quaternion projectileRotation = Quaternion.LookRotation(firePoint.forward) * deviation;

        // Instantiate and fire your projectile
        GameObject projectile = ObjectPoolManager.ins.GetFromPool("Projectile", firePoint.position, projectileRotation);
        projectile.SetActive(true);
        projectile.tag = "Projectile";
        projectile.GetComponent<Projectile>().SetShooterTag(owner);
    }
}
