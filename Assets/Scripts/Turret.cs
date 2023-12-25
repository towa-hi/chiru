using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class Turret : Enemy
{


    public void Awake()
    {

    }
    /**
    public override void OnDamaged(float damage, Vector3 damageSourcePosition)
    {
        // Debug.Log("im just not gonna do it");
        if (!isInvincible)
        {
            ApplyDamage(damage);
        }
        Debug.Log("I took damage");
    }
    **/
    
    protected override void PrepareAttackTarget()
    {
        if (currentTarget == null)
        {
            return;
        }
        
        // Calc dir to target
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        directionToTarget.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        if (IsFacingTarget() && IsWithinAttackRange())
        {
            if (isReadyToFire)
            AttackTarget();
        }
    }

    bool isReadyToFire = true;
    
    protected override void AttackTarget()
    {
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        Debug.Log("shooting");
        Shoot();
        StartCoroutine(ResetFire());
    }

    public GameObject projectileSpawner;
    void Shoot()
    {
        Quaternion projectileRotation = Quaternion.LookRotation(projectileSpawner.transform.forward);
        GameObject projectile = ObjectPoolManager.ins.GetFromPool("Projectile", projectileSpawner.transform.position,
            projectileRotation);
        projectile.SetActive(true);
        projectile.tag = "Projectile";
        projectile.GetComponent<Projectile>().SetShooterTag(this);
    }

    IEnumerator ResetFire()
    {
        isReadyToFire = false;
        yield return new WaitForSeconds(0.5f);
        isReadyToFire = true;
    }

}
