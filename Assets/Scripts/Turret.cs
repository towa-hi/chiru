using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class Turret : Enemy
{
    [SerializeField] private Gun gun;

    public void Awake()
    {
        gun.SetFireOrigin(transform);
        gun.objectPoolManager = ObjectPoolManager.ins;
    }

    public override void OnDamaged(float damage, Vector3 damageSourcePosition)
    {
        // Debug.Log("im just not gonna do it");
    }
    
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
            if (gun.fireDestination != currentTarget.transform)
            {
                gun.SetFireDestination(currentTarget.transform);   
            }
            AttackTarget();
        }
    }

    protected override void AttackTarget()
    {
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("Idle"))
        {
            handsController.SetTrigger("AttackTrigger");
            if (gun)
            {
                gun.Shoot();    
            }
        }
    }
}
