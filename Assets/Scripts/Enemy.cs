using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Entity
{
    public float hearingRadius;
    public float visionRadius;
    public float visionFOV;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    
    public GameObject currentTarget;
    public float targetLostDelay = 1f;
    public NavMeshAgent navMeshAgent;

    
    void Update()
    {
        FindVisibleTargets();
        if (currentTarget)
        {
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }

        MoveTowardsTarget();
        Debug.DrawLine(transform.position, navMeshAgent.destination, Color.blue);

    }
    void FindVisibleTargets()
    {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRadius, targetMask);
        bool targetFound = false;
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            // if target is not player
            if (!target.CompareTag("PlayerHurtbox"))
            {
                continue;
            }
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            // if target is not within fov 
            if (!(Vector3.Angle(transform.forward, dirToTarget) < visionFOV / 2))
            {
                continue;
            }
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            // if target is behind a obstacle
            if (Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
            {
                continue;
            }

            if (target.root.gameObject != currentTarget)
            {
                currentTarget = target.root.gameObject;
                Debug.Log(name + " AI: I found target!");
            }
            targetFound = true;
        }
        // if no targets within vision 
        if (!targetFound && currentTarget)
        {
            LoseTarget();
        }
    }

    public float minimumDistanceToTarget;
    void MoveTowardsTarget()
    {
        if (currentTarget)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToTarget > minimumDistanceToTarget)
            {
                Vector3 forwardDirection = (currentTarget.transform.position - transform.position).normalized;

                forwardDirection.Normalize(); // Normalize to ensure consistent speed

                navMeshAgent.SetDestination(transform.position + forwardDirection * 50 * Time.deltaTime);
                navMeshAgent.isStopped = false;
            }
            else
            {
                navMeshAgent.isStopped = true;
            }
        }
        else
        {
            navMeshAgent.ResetPath();
        }
    }

    bool isLosingTarget;
    void LoseTarget()
    {
        if (!isLosingTarget)
        {
            isLosingTarget = true;
            Debug.Log(name + " AI: I lost target!");
            StartCoroutine(LoseTargetAfterDelay());
        }
    }

    IEnumerator LoseTargetAfterDelay()
    {
        yield return new WaitForSeconds(targetLostDelay);
        currentTarget = null;
        isLosingTarget = false;
        Debug.Log(name + " AI: I no longer have currentTarget");
    }

}
