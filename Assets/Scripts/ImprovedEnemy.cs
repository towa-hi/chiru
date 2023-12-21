using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ImprovedEnemy : Entity
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

    [SerializeField] float distanceToTarget = float.MaxValue;

}
