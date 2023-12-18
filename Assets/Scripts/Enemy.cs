using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float hearingRadius;
    public float visionRadius;
    public float visionFOV;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    
    public float hp;
    public float movespeed;
    public CharacterController characterController;
    public List<GameObject> deleteAfterDeath;
    public GameObject currentTarget;
    public float targetLostDelay = 1f;
    void OnDeath()
    {
        
    }

    void Update()
    {
        FindVisibleTargets();
        if (currentTarget != null)
        {
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }
    }
    void FindVisibleTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, targetMask);
        bool targetFound = false;
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (target.root.gameObject == currentTarget)
            {
                targetFound = true;
                break;
            }
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < visionFOV / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    // Target is within vision and not obstructed
                    Debug.Log("Target in sight: " + target.root.name);
                    currentTarget = target.root.gameObject;
                    targetFound = true;
                }
            }
            // BUG: FIGURE OUT WHY WE DONT LOSE VISIBLE TARGETS PROPERLY WHEN SPAWNING NEXT TO PLAYER
        }

        if (!targetFound)
        {
            Debug.Log("no targets found");
            if (currentTarget != null)
            {
                LoseTarget();
            }
        }
    }

    void LoseTarget()
    {
        StartCoroutine(LoseTargetAfterDelay());
    }

    IEnumerator LoseTargetAfterDelay()
    {
        yield return new WaitForSeconds(targetLostDelay);
        currentTarget = null;
    }
}
