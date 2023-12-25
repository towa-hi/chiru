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
    public float rotationSpeed;
    public Animator handsController;
    public bool ignoresObstacles;
    public float minSeparationDistance = 1.5f;

    Vector3 combinedDirection;
    
    float distanceToCurrentTarget = float.MaxValue;

    void Start()
    {
        navMeshAgent.updateRotation = false;
    }
    void Update()
    {
        if (isDead)
        {
            return;
        }
        FindVisibleTargets();
        LookAtTarget();
        EvaluateContexts();
        MoveTowardsDestination();
        HandleWeaponEffect();
        
        Debug.DrawLine(transform.position, navMeshAgent.destination, Color.blue);
    }

    void LookAtTarget()
    {
        if (isParried)
        {
            return;
        }
        if (currentTarget)
        {
            // Calc dir to target
            Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
            directionToTarget.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void HandleWeaponEffect()
    {
        // handle weapon effects
        if (attackEffect && meleeWeapon)
        {
            if (meleeWeapon.currentAttackPhase == AttackPhase.ATTACKING)
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
            if (ignoresObstacles)
            {
                if (Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    continue;
                }
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

    [SerializeField] float walkTowardsTargetWeight;
    [SerializeField] float separationWeight;
    
    void EvaluateContexts()
    {
        List<AIContext> contexts = new List<AIContext>();
        if (currentTarget)
        {
            distanceToCurrentTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToCurrentTarget <= attackRange)
            {
                PrepareAttackTarget();
            }
            else
            {
                Vector3 targetDirection = (currentTarget.transform.position - transform.position).normalized;
                
                contexts.Add(new AIContext(targetDirection, walkTowardsTargetWeight)); // Higher weight for target direction

                // Separation context
                Vector3 separationVector = CalculateSeparationVector();
                contexts.Add(new AIContext(separationVector, separationWeight)); // Adjust weight as needed

                // Combine contexts
                combinedDirection = CombineContexts(contexts);
            }
        }
    }
    
    void MoveTowardsDestination()
    {
        if (isKnockedBack)
        {
            Vector3 newPosition = transform.position + knockbackVector * Time.deltaTime;
            Debug.DrawLine(transform.position, newPosition);
            navMeshAgent.Warp(newPosition);
        }
        else
        {
            if (isParried)
            {
                // stop all movement
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }
            else if (currentTarget && distanceToCurrentTarget > attackRange)
            {
                navMeshAgent.SetDestination(transform.position + combinedDirection * distanceToCurrentTarget);
                navMeshAgent.isStopped = false;
            }
            else
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }
        }
    }

    bool isLosingTarget;
    void LoseTarget()
    {
        if (!isLosingTarget)
        {
            isLosingTarget = true;
            //Debug.Log(name + " AI: I lost target!");
            StartCoroutine(LoseTargetAfterDelay());
        }
    }

    IEnumerator LoseTargetAfterDelay()
    {
        yield return new WaitForSeconds(targetLostDelay);
        currentTarget = null;
        isLosingTarget = false;
        //Debug.Log(name + " AI: I no longer have currentTarget");
    }

    protected virtual void PrepareAttackTarget()
    {
        if (!currentTarget)
        {
            return;
        }
        if (currentTarget.GetComponent<Entity>().isInvincible)
        {
            Debug.Log("Waiting for not invincible");
            return;
        }
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
            float chance = UnityEngine.Random.Range(0f, 1f);
            Debug.Log(chance);
            if (chance >= 0.2f)
            {
                handsController.SetTrigger("AttackTrigger");
            }
            else
            {
                handsController.SetTrigger("SlowAttackTrigger");
            }
            
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
        return angleToTarget < 20f; // Adjust this value as needed
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
    
    public override void OnDamaged(GameObject source, float damage, Vector3 damageSourcePosition, float knockbackMagnitude, float knockbackDuration, bool applyInvincibility)
    {
        base.OnDamaged(source, damage, damageSourcePosition, knockbackMagnitude, knockbackDuration, applyInvincibility);

        if (!isDead)
        {
            currentTarget = source.GetComponentInParent<Entity>().gameObject;
            Debug.Log("Set currentTarget to " + currentTarget);
        }
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