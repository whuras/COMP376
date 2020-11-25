using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class FlyingEnemyAI : MonoBehaviour
{
    
    private NavMeshAgent agent;
    private Transform player;
    private HealthController healthController;
    
    [Header("Behavior Toggles")] 
    
    [Tooltip("Whether the game object wanders the player when the player is out of range.")]
    public bool CanWander;
    
    [Tooltip("Whether the game object chases the player when they are in range.")]
    public bool CanChasePlayer;
    
    [Tooltip("Whether the game object attacks the player when they are in range.")]
    public bool CanAttackPlayer;
    
    [Tooltip("Whether the game object retreats from the player when they are attacked.")]
    public bool CanRetreat;

    public Projectile Projectile;
    
    
    [Header("Chasing Variables")] 
    
    [Tooltip("The minimum distance that will be maintained between the player and the enemy when calculating a target" +
             " destination.")]
    public float PersonalSpace;
    
    [Tooltip("The delay between each time the enemy recalculates their chase destination.")]
    public float ChaseDestinationComputeInterval;
    
    [Tooltip("Multiplier for how fast the enemy will snap to facing the players position.")]
    public float SnapToMultiplier;

    [Tooltip("Minimum distance from the player to start chasing.")]
    public float SightRange;
    
    private float mChaseTimeout;


    [Header("Wander Variables")] 
    
    public float WanderDistanceMin;
    public float WanderDistanceMax;

    [Tooltip("The delay between each time the enemy recalculates their wander destination.")]
    public float WanderDestinationComputeInterval;

    private float mWanderTimeout;
    
    
    [Header("Attacking Variables")]
    [Tooltip("Amount of time between projectile launches.")]
    public float TimeBetweenAttacks;

    [Tooltip("Amount of damage each projectile inflicts.")]
    public float AttackDamage;
    
    [Tooltip("Minimum distance from the player for the enemy to start attacking.")]
    public float AttackRange;
    
    private float mTimeLastAttack;

    
    [Header("Retreat Variables")] 
    [Tooltip("The odds that an enemy will retreat after being hit (must be [0,1]).")]
    public float RetreatProbability;

    [Tooltip("Positions to where the flying enemy will retreat to.")]
    public GameObject[] RetreatPositions;
    
    [Tooltip("Number of seconds the enemy will retreat for before taking another action.")]
    public float RetreatDuration;

    private float mRetreatTimeout;
    
    
    // States
    private bool mIsAlive;
    private bool mWasHitLastFrame;

    [FormerlySerializedAs("gunPoint")] public GameObject GunPoint;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.baseOffset = 5;
        healthController = GetComponent<HealthController>();

        healthController.OnDeath += OnDeath;
        healthController.OnDamaged += OnDamaged;
        
        mChaseTimeout = -1;
        mRetreatTimeout = -RetreatDuration;

        mIsAlive = true;
        mWasHitLastFrame = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (mIsAlive && Time.time - mRetreatTimeout > 5)
        {
            agent.updateRotation = true;

            bool playerInSightRange = Vector3.Distance(player.transform.position, transform.position) < SightRange;
            bool playerInAttackRange = Vector3.Distance(player.transform.position, transform.position) < AttackRange;

            if (!playerInSightRange && !playerInAttackRange && CanWander)
            {
                Wander();
            }
            
            if (playerInSightRange && CanChasePlayer)
            {
                ChasePlayer();
            }
            
            if (playerInAttackRange && playerInSightRange && CanAttackPlayer)
            {
                AttackPlayer();
            }
            
            if (mWasHitLastFrame && Random.value <= RetreatProbability && CanRetreat)
            {
                RetreatPlayer();
            }

            mWasHitLastFrame = false;
        }
    }

    private void AttackPlayer()
    {
        // Attack the player on a given interval
        // TODO Maybe make this interval random?
        if (Time.time - mTimeLastAttack > TimeBetweenAttacks)
        {
            mTimeLastAttack = Time.time;
            Projectile newProjectile = Instantiate(Projectile, GunPoint.transform.position, Quaternion.LookRotation(player.transform.position - GunPoint.transform.position));
            newProjectile.Damage = AttackDamage;
            newProjectile.Owner = gameObject;    
        }
    }

    private void ChasePlayer()
    {
        // Change navigation destination every _rescanTime seconds
        if (Time.time - mChaseTimeout > ChaseDestinationComputeInterval)
        {
            mChaseTimeout = Time.time;
            
            Vector3 playerPosition = player.transform.position;

            float angle = Random.Range(0F, 360F);

            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * PersonalSpace;
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * PersonalSpace;

            Vector3 destination = new Vector3(playerPosition.x + x, playerPosition.y, playerPosition.z + z);
            agent.SetDestination(destination);
        }
        
        // If the agent is chasing, then disable automatic navmesh face direction and 
        agent.updateRotation = false;

        // Make the enemy look at the player when they are attacking
        Vector3 direction = player.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, SnapToMultiplier * Time.deltaTime);
    }

    private void RetreatPlayer()
    {
        if (mRetreatTimeout - Time.time <= RetreatDuration)
        {
            GetComponent<NavMeshAgent>().enabled = false;
            GameObject retreatPosition = RetreatPositions[Random.Range(0, RetreatPositions.Length - 1)];
            transform.position = retreatPosition.transform.position;
            transform.LookAt(player.transform.position - retreatPosition.transform.position);
            mRetreatTimeout = Time.time;   
        }
    }

    private void Wander()
    {
        // Change navigation destination every _rescanTime seconds
        if (Time.time - mWanderTimeout > WanderDestinationComputeInterval)
        {
            // Compute the retreat distance
            float wanderDistance = Random.Range(WanderDistanceMin, WanderDistanceMax);

            // Compute the retreat angle
            float wanderAngle = Random.Range(0, 360);

            // Compute the x,z coordinates for our wander distance
            float z = Mathf.Sin(Mathf.Deg2Rad * wanderAngle) * (wanderDistance);
            float x = Mathf.Cos(Mathf.Deg2Rad * wanderAngle) * (wanderDistance);

            // Compute destination position
            Vector3 destination = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

            // Set destination
            agent.SetDestination(destination);

            mWanderTimeout = Time.time;
        }
    }

    private void OnDeath()
    {
        agent.isStopped = true;
        mIsAlive = false;
    }

    private void OnDamaged()
    {
        mWasHitLastFrame = true;
    }
}
