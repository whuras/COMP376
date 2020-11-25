using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyAI : MonoBehaviour
{
    
    private NavMeshAgent agent;
    private Transform player;
    private HealthController healthController;
    
    [Header("Behavior Toggles")] 
    
    [Tooltip("Whether the game object wanders the player when the player is out of range.")]
    public bool wander;
    
    [Tooltip("Whether the game object chases the player when they are in range.")]
    public bool chasePlayer;
    
    [Tooltip("Whether the game object attacks the player when they are in range.")]
    public bool attackPlayer;
    
    [Tooltip("Whether the game object retreats from the player when they are attacked.")]
    public bool retreatPlayer;

    public Projectile Projectile;
    
    
    [Header("Chasing Variables")] 
    
    [Tooltip("The minimum distance that will be maintained between the player and the enemy when calculating a target" +
             " destination.")]
    public float personalSpace;
    
    [Tooltip("The delay between each time the enemy recalculates their chase destination.")]
    public float chaseDestinationComputeInterval;
    
    [Tooltip("Multiplier for how fast the enemy will snap to facing the players position.")]
    public float snapToMultiplier;

    [Tooltip("Minimum distance from the player to start chasing.")]
    public float sightRange;
    
    private float _chaseTimeout;


    [Header("Wander Variables")] 
    
    public float wanderDistanceMin;
    public float wanderDistanceMax;

    [Tooltip("The delay between each time the enemy recalculates their wander destination.")]
    public float wanderDestinationComputeInterval;

    private float _wanderTimeout;
    
    
    [Header("Attacking Variables")]
    [Tooltip("Amount of time between projectile launches.")]
    public float timeBetweenAttacks;

    [Tooltip("Amount of damage each projectile inflicts.")]
    public float attackDamage;
    
    [Tooltip("Minimum distance from the player for the enemy to start attacking.")]
    public float attackRange;
    
    private float _timeLastAttack;

    
    [Header("Retreat Variables")] 
    [Tooltip("The odds that an enemy will retreat after being hit (must be [0,1]).")]
    public float retreatProbability;

    [Tooltip("How wide the triangle drawn behind the enemy used to determine where they will retreat to (must be [0,90]).")]
    public float retreatAngleWidth;
    
    [Tooltip("Number of seconds the enemy will retreat for before taking another action.")]
    public float retreatDuration;

    public float retreatDistanceMin;
    public float retreatDistanceMax;
    
    private float _retreatTimeout;
    
    
    // States
    private bool _isAlive;
    private bool _wasHitLastFrame;

    public GameObject gunPoint;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        healthController = GetComponent<HealthController>();

        healthController.OnDeath += OnDeath;
        healthController.OnDamaged += OnDamaged;
        
        _chaseTimeout = -1;
        _retreatTimeout = -retreatDuration;

        _isAlive = true;
        _wasHitLastFrame = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isAlive && Time.time - _retreatTimeout > 5)
        {
            agent.updateRotation = true;

            bool playerInSightRange = Vector3.Distance(player.transform.position, transform.position) < sightRange;
            bool playerInAttackRange = Vector3.Distance(player.transform.position, transform.position) < attackRange;

            if (!playerInSightRange && !playerInAttackRange && wander)
            {
                Wander();
            }

            if (playerInSightRange && chasePlayer)
            {
                ChasePlayer();
            }

            if (playerInAttackRange && playerInSightRange && attackPlayer)
            {
                AttackPlayer();
            }

            if (_wasHitLastFrame && Random.value <= retreatProbability && retreatPlayer)
            {
                RetreatPlayer();
            }

            _wasHitLastFrame = false;
        }
    }

    private void AttackPlayer()
    {
        // Attack the player on a given interval
        // TODO Maybe make this interval random?
        if (Time.time - _timeLastAttack > timeBetweenAttacks)
        {
            _timeLastAttack = Time.time;
            Projectile newProjectile = Instantiate(Projectile, gunPoint.transform.position, Quaternion.LookRotation(player.transform.position - gunPoint.transform.position));
            newProjectile.Damage = attackDamage;
            newProjectile.Owner = gameObject;    
        }
    }

    private void ChasePlayer()
    {
        // Change navigation destination every _rescanTime seconds
        if (Time.time - _chaseTimeout > chaseDestinationComputeInterval)
        {
            _chaseTimeout = Time.time;
            
            Vector3 playerPosition = player.transform.position;

            float angle = Random.Range(0F, 360F);

            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * personalSpace;
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * personalSpace;

            Vector3 destination = new Vector3(playerPosition.x + x, playerPosition.y, playerPosition.z + z);
            agent.SetDestination(destination);
        }
        
        // If the agent is chasing, then disable automatic navmesh face direction and 
        agent.updateRotation = false;

        // Make the enemy look at the player when they are attacking
        Vector3 direction = player.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, snapToMultiplier * Time.deltaTime);
    }

    private void RetreatPlayer()
    {
        // Compute the retreat distance
        float retreatDistance = Random.Range(retreatDistanceMin, retreatDistanceMax);

        // Compute the retreat angle
        float retreatAngle = Random.Range(270 - retreatAngleWidth, 270 + retreatAngleWidth);

        // Compute the x,z coordinates for our retreat position
        float z = Mathf.Sin(Mathf.Deg2Rad * retreatAngle) * (transform.forward.z * retreatDistance);
        float x = Mathf.Cos(Mathf.Deg2Rad * retreatAngle) * (transform.forward.x * retreatDistance);

        // Compute destination position
        Vector3 destination = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);


        // Set destination
        agent.SetDestination(destination);
        
        // Set a timeout time so that no other actions are taken during retreat
        _retreatTimeout = Time.time;
        
        // Reset last frame his indicator
        _wasHitLastFrame = false;
    }

    private void Wander()
    {
        // Change navigation destination every _rescanTime seconds
        if (Time.time - _wanderTimeout > wanderDestinationComputeInterval)
        {
            // Compute the retreat distance
            float wanderDistance = Random.Range(wanderDistanceMin, wanderDistanceMax);

            // Compute the retreat angle
            float wanderAngle = Random.Range(0, 360);

            // Compute the x,z coordinates for our wander distance
            float z = Mathf.Sin(Mathf.Deg2Rad * wanderAngle) * (wanderDistance);
            float x = Mathf.Cos(Mathf.Deg2Rad * wanderAngle) * (wanderDistance);

            // Compute destination position
            Vector3 destination = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

            // Set destination
            agent.SetDestination(destination);

            _wanderTimeout = Time.time;
        }
    }

    private void OnDeath()
    {
        agent.isStopped = true;
        _isAlive = false;
    }

    private void OnDamaged()
    {
        _wasHitLastFrame = true;
    }
}
