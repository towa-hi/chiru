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
    public float targetLostDelay = 5f;
    public NavMeshAgent navMeshAgent;
    public MeleeWeapon meleeWeapon;
    
    public Lighsaber attackEffect;

    public float attackRange;
    public float safeDistanceFromObstacle = 2f;
    public float rotationSpeed;
    public Animator handsController;

    public float minSeparationDistance = 1.5f;

    Vector3 combinedDirection;
    
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

    void EvaluateContexts()
    {
        if (currentTarget)
        {
            distanceToCurrentTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToCurrentTarget <= attackRange)
            {
                PrepareAttackTarget();
            }
            else
            {
                // Create a list of contexts
                List<AIContext> contexts = new List<AIContext>();
                Vector3 dirToTarget = (currentTarget.transform.position - transform.position).normalized;
                contexts.Add(new AIContext(dirToTarget, 1.0f)); // Higher weight for target direction

                // Separation context
                Vector3 separationVector = CalculateSeparationVector();
                contexts.Add(new AIContext(separationVector, 0.5f)); // Adjust weight as needed

                // Combine contexts
                combinedDirection = CombineContexts(contexts);

            }
        }
    }
    public float minimumDistanceToTarget;
    void MoveTowardsTarget()
    {
        if (currentTarget)
        {
            if (distanceToCurrentTarget > attackRange)
            {
                navMeshAgent.SetDestination(transform.position + combinedDirection * distanceToCurrentTarget);
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

    protected virtual void PrepareAttackTarget()
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

    protected virtual void AttackTarget()
    {
        AnimatorStateInfo currentState = handsController.GetCurrentAnimatorStateInfo(0);
        if (currentState.IsName("Idle"))
        {
            handsController.SetTrigger("AttackTrigger");
        }
    }

    public bool IsFacingTarget()
    {
        if (currentTarget == null)
        {
            return false;
        }
        
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
        return angleToTarget < 10f; // Adjust this value as needed
    }
    
    public bool IsWithinAttackRange()
    {
        if (currentTarget == null)
        {
            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        return distanceToTarget <= attackRange;
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
            }
        }

        if (neighbors > 0)
        {
            separationVector /= neighbors; // Average the separation
        }

        return separationVector;
    }
    
    Vector3 CombineContexts(List<AIContext> contexts)
    {
        Vector3 combinedDirection = Vector3.zero;
        float totalWeight = 0f;

        foreach (AIContext context in contexts)
        {
            combinedDirection += context.direction.normalized * context.weight;
            totalWeight += context.weight;
        }

        if (totalWeight > 0f)
        {
            combinedDirection /= totalWeight; // Normalize combined direction by total weight
        }

        return combinedDirection;
    }

    public float knockbackStrength = 1f;
    public float knockbackDuration = 1f;
    bool isKnockedBack = false;

    public override void OnDamaged(float damage, Vector3 damageSourcePosition)
    {
        base.OnDamaged(damage, damageSourcePosition);
        Debug.Log("I took damage");
        isKnockedBack = true;
        isInvincible = true;
        Debug.Log("isInvincible true");
        handsController.Play("Idle");
        StartCoroutine(KnockbackRoutine(damageSourcePosition));
    }

    IEnumerator KnockbackRoutine(Vector3 damageSourcePosition)
    {
        Vector3 knockbackDirection = (transform.position - damageSourcePosition).normalized;
        Vector3 potentialKnockbackDestination = transform.position + knockbackDirection * knockbackStrength;

        // Check if the destination is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialKnockbackDestination, out hit, knockbackStrength, NavMesh.AllAreas))
        {
            // Apply knockback if the destination is valid
            float timer = 0;
            while (timer < knockbackDuration)
            {
                timer += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, hit.position, timer / knockbackDuration);
                yield return null;
            }
        }
        else
        {
            // If no valid position is found, don't apply knockback
            Debug.LogWarning("No valid knockback position found on NavMesh.");
        }

        // Once the knockback is complete, reset the NavMeshAgent's destination
        if (navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(transform.position);
        }

        isKnockedBack = false;
        isInvincible = false;
        Debug.Log("isInvincible false");
    }


}
public class AIContext
{
    public Vector3 direction;
    public float weight;

    public AIContext(Vector3 direction, float weight)
    {
        this.direction = direction;
        this.weight = weight;
    }
}