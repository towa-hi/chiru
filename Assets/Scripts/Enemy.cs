using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public float hearingRadius;
    public float visionRadius;
    public float visionFOV;
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    
    public float hp;
    public float moveSpeed;
    public float rotationSpeed;
    public CharacterController characterController;
    public List<GameObject> deleteAfterDeath;
    public GameObject currentTarget;
    public float targetLostDelay = 1f;
    public NavMeshAgent navMeshAgent;

    void Start()
    {
        navMeshAgent.speed = moveSpeed;
    }
    void OnDeath()
    {
        
    }

    void Update()
    {
        FindVisibleTargets();
        if (currentTarget != null)
        {
            //Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }

        //TurnTowardsTarget();
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
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            // if target is within fov 
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
        }

        if (!targetFound)
        {
            Debug.Log("no targets found");
            if (currentTarget is not null)
            {
                LoseTarget();
            }
        }
    }

    void TurnTowardsTarget()
    {
        if (currentTarget is not null)
        {
            Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
            // Remove any vertical component in the direction
            directionToTarget.y = 0;

            // Calculate the rotation required to face the target
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            // Rotate the enemy smoothly towards the target
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        }
    }

    public float minimumDistanceToTarget;
    public bool movesIrregularly;
    void MoveTowardsTarget()
    {
        if (currentTarget is not null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToTarget > minimumDistanceToTarget)
            {
                Vector3 forwardDirection = (currentTarget.transform.position - transform.position).normalized;
                Vector3 strafeDirection = Vector3.zero;

                // if (movesIrregularly)
                // {
                //     float strafeIntensity = Mathf.Clamp01((distanceToTarget - minimumDistanceToTarget) / visionRadius);
                //     strafeDirection = GetStrafeDirection() * strafeIntensity;
                // }

                Vector3 combinedDirection = forwardDirection + strafeDirection;
                combinedDirection.Normalize(); // Normalize to ensure consistent speed

                navMeshAgent.SetDestination(transform.position + combinedDirection * moveSpeed * Time.deltaTime);
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

    float strafeTimer;
    Vector3 strafeDirection;
    Vector3 GetStrafeDirection()
    {
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0)
        {
            strafeDirection = (Random.Range(0, 2) * 2 - 1) * transform.right;
            strafeTimer = Random.Range(0.1f, 0.3f);
        }

        return strafeDirection;
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
