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
    public MeleeWeapon meleeWeapon;
    
    public Lighsaber attackEffect;

    public float attackRange;
    public float safeDistanceFromObstacle = 2f;
    public float rotationSpeed;
    public Animator handsController;

    public float minSeparationDistance = 1.5f;
    
    float distanceToCurrentTarget = float.MaxValue;
    
    void Update()
    {
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("Idle"))
        {
            FindVisibleTargets();
            EvaluateContexts();
            MoveTowardsTarget();
        }
        HandleWeaponEffect();
        Debug.DrawLine(transform.position, navMeshAgent.destination, Color.blue);
    }

    void HandleWeaponEffect()
    {
        // handle weapon effects
        if (attackEffect && meleeWeapon)
        {
            if (meleeWeapon.currentAttackPhase == AttackPhase.ATTACKING ||
                meleeWeapon.currentAttackPhase == AttackPhase.WINDDOWN)
            {
                attackEffect.SetEnabled(true);
            }
            else
            {
                attackEffect.SetEnabled(false);
            }
        }
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
                //continue;
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

    void EvaluateContexts()
    {
        if (currentTarget)
        {
            distanceToCurrentTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToCurrentTarget <= attackRange)
            {
                PrepareAttackTarget();
            }
        }
    }
    public float minimumDistanceToTarget;
    void MoveTowardsTarget()
    {
        if (currentTarget)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToTarget > attackRange)
            {
                Vector3 dirToTarget = (currentTarget.transform.position - transform.position).normalized;
                Vector3 separationVector = CalculateSeparationVector();

                // Apply a weight to the separation vector
                float separationWeight = 0.5f; // Adjust this value to control the influence of separation
                Vector3 weightedSeparation = separationVector.normalized * separationWeight;

                // Combine the direction to the target with the weighted separation vector
                Vector3 combinedDirection = dirToTarget + weightedSeparation;

                // Calculate the final destination
                Vector3 finalDestination = transform.position + combinedDirection * distanceToTarget;

                navMeshAgent.SetDestination(finalDestination);
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
        Vector3 CalculateSeparationVector()
        {
            Vector3 separationVector = Vector3.zero;
            int neighbors = 0;

            Collider[] hits = Physics.OverlapSphere(transform.position, minSeparationDistance);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("EnemyHurtbox") && !hit.transform.IsChildOf(transform))
                {
                    Vector3 awayFromNeighbor = transform.position - hit.transform.position;
                    separationVector += awayFromNeighbor;
                    neighbors++;
                    Debug.Log("added neighbor:" + hit.GetComponentInParent<Entity>().gameObject.name);
                }
            }

            if (neighbors > 0)
            {
                separationVector /= neighbors; // Average the separation
            }

            return separationVector;
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

    void PrepareAttackTarget()
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
            AttackTarget();
        }
    }

    void AttackTarget()
    {
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("Idle"))
        {
            handsController.SetTrigger("AttackTrigger");
        }
    }

    bool IsFacingTarget()
    {
        if (currentTarget == null)
        {
            return false;
        }
        
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        return angleToTarget < 10f; // Adjust this value as needed
    }
    
    bool IsWithinAttackRange()
    {
        if (currentTarget == null)
        {
            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        return distanceToTarget <= attackRange;
    }

    class Context
    {
        public Vector3 direction;
        public float weight;

        public Context(Vector3 direction, float weight)
        {
            this.direction = direction;
            this.weight = weight;
        }
    }
}
