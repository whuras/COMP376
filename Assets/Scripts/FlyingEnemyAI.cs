using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class FlyingEnemyAI : MonoBehaviour
{

    private NavMeshAgent agent;
    private Transform player;
    private HealthController healthController;
    private Animator animator;

    [Header("Behavior Toggles")]
    [Tooltip("Whether the game object wanders the player when the player is out of range.")]
    public bool CanWander;

    [Tooltip("Whether the game object chases the player when they are in range.")]
    public bool CanChasePlayer;

    [Tooltip("Whether the game object attacks the player when they are in range.")]
    public bool CanSwoopAttackPlayer;

    [Tooltip("Whether the game object attacks the player when they are in range.")]
    public bool CanRangedAttackPlayer;

    [Tooltip("Whether the game object retreats from the player when they are attacked.")]
    public bool CanRetreat;

    public Projectile Projectile;

    #region Chase parameters 
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
#endregion

    #region Wander parameters
    [Header("Wander Variables")] public float WanderDistanceMin;
    public float WanderDistanceMax;

    [Tooltip("The delay between each time the enemy recalculates their wander destination.")]
    public float WanderDestinationComputeInterval;

    private float mWanderTimeout;
#endregion

    #region Swoop Attack parameters
    [Header("Swoop Attack Variables")]
    
    [Tooltip(
        "Time taken for the second half of the swoop attack to occur (when the ai retreats away from the player).")]
    public float SwoopAttackAscentDuration;

    [Tooltip("Time taken for the first half of the swoop attack to occur (when the ai approaches the player).")]
    public float SwoopAttackDescentDuration;

    [Tooltip("Amount of damage each projectile inflicts.")]
    public float SwoopAttackDamage;

    [Tooltip("Minimum distance from the player for the enemy to start a swoop attack.")]
    public float SwoopAttackRange;

    [Tooltip("Cooldown time between swoop attacks.")]
    public float SwoopAttackCooldown;

    private float mTimeSwoopAttackStarted;
    private Vector3 mSwoopAttackStartLocation;
    private Vector3 mSwoopAttackLocation;
    private Vector3 mSwoopRetreatLocation;

    private bool mIsSwoopAttacking
    {
        get { return Time.time - mTimeSwoopAttackStarted <= SwoopAttackAscentDuration + SwoopAttackDescentDuration; }
    }

    private bool mIsSwoopAttackAscending
    {
        get
        {
            return !mIsSwoopAttackDescending && Time.time - mTimeSwoopAttackStarted <=
                (SwoopAttackAscentDuration + SwoopAttackDescentDuration);
        }
    }

    private bool mIsSwoopAttackDescending
    {
        get { return Time.time - mTimeSwoopAttackStarted <= SwoopAttackDescentDuration; }
    }

    private bool mSwoopAttackCooldownElapsed
    {
        get
        {
            return (Time.time - mTimeSwoopAttackStarted) - (SwoopAttackAscentDuration + SwoopAttackDescentDuration) -
                SwoopAttackCooldown >= 0;
        }
    }
    #endregion
    
    #region Ranged Attack parameters
    
    [Header("Ranged Attack Variables")] 
    
    [Tooltip("Minimum distance from the player for the enemy to start a ranged attack.")]
    public float RangedAttackRange;

    public float RangedAttackDamage;

    public float RangedAttackCooldown;
    
    public GameObject GunPoint;

    private float mTimeRangedAttackStarted;
    
    private bool mRangedAttackCooldownElapsed
    {
        get
        {
            return (Time.time - mTimeRangedAttackStarted) - RangedAttackCooldown >= 0;
        }
    }
    
    #endregion

    #region Retreat parameters
    
    [Header("Retreat Variables")] 
    
    [Tooltip("The odds that an enemy will retreat after being hit (must be [0,1]).")]
    public float RetreatProbability;

    [Tooltip("Positions to where the flying enemy will retreat to.")]
    public GameObject[] RetreatPositions;

    [Tooltip("Number of seconds the enemy will retreat for before taking another action.")]
    public float RetreatDuration;

    [Tooltip("Number of seconds the enemy will take to travel to destination retreat point.")]
    public float RetreatTravelDuration;

    public float RetreatXAmplitude;
    public float RetreatYAmplitude;

    public float RetreatXFrequency;
    public float RetreatYFrequency;

    // Time at which the retreat started
    private float mTimeRetreatStarted;

    // Position where the enemy started to retreat
    private Vector3 mRetreatStartPosition;

    // Retreat position chosen
    private GameObject mRetreatEndPosition;

    // Whether the enemy is actively retreating (traveling or idle at retreat point)
    private bool mIsRetreating
    {
        get { return Time.time - mTimeRetreatStarted <= RetreatDuration; }
    }

    // Whether the enemy is actively traveling to retreat location
    private bool mIsTravellingToRetreat
    {
        get { return Time.time - mTimeRetreatStarted <= RetreatTravelDuration; }
    }

    // Whether the enemy has already arrived and is idling at the retreat location
    private bool mIsRetreated
    {
        get
        {
            float time = Time.time - mTimeRetreatStarted;
            return time < RetreatDuration && time >= RetreatTravelDuration;
        }
    }

    #endregion

    // States
    private bool mIsAlive;
    private bool mWasHitLastFrame;
    

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.baseOffset = 5;
        healthController = GetComponent<HealthController>();
        animator = gameObject.GetComponentInChildren<Animator>();

        healthController.OnDeath += OnDeath;
        healthController.OnDamaged += OnDamaged;

        mChaseTimeout = -1;
        mTimeRetreatStarted = -RetreatDuration;
        mTimeSwoopAttackStarted = -SwoopAttackAscentDuration + -SwoopAttackDescentDuration + -1;

        mIsAlive = true;
        mWasHitLastFrame = false;
    }
    
    
    // Update is called once per frame
    void Update()
    {
        if (mIsAlive)
        {
            agent.updateRotation = true;
            agent.enabled = true;

            bool playerInSightRange = Vector3.Distance(player.transform.position, transform.position) < SightRange;
            bool playerInSwoopAttackRange =
                Vector3.Distance(player.transform.position, transform.position) < SwoopAttackRange;
            bool playerInRangedAttackRange =
                Vector3.Distance(player.transform.position, transform.position) < RangedAttackRange;
            
            /* Wander */            
            if (!playerInSightRange && !playerInSwoopAttackRange && !playerInRangedAttackRange && CanWander)
            {
                Wander();
            }

            /* Chasing */
            if (playerInSightRange && !mIsRetreated && CanChasePlayer)
            {
                ChasePlayer();
            }

            /* Swoop attack */
            if (CanSwoopAttackPlayer)
            {
                if (mIsSwoopAttacking)
                {
                    SwoopAttack();
                }
                else if (playerInSwoopAttackRange)
                {
                    SetupSwoopAttack();
                }
            }

            /* Ranged attack */
            if (CanRangedAttackPlayer && playerInRangedAttackRange)
            {
                RangedAttack();
            }
            
            
            /* Retreat */
            if (CanRetreat)
            {
                // If a retreat has been started, then continue the action
                if (mIsRetreating)
                {
                    Retreat();
                }
                // If no retreat is in progress, then set one up
                else
                {
                    SetupRetreat();
                }
            }

        }
        
        UpdateAnimatorParameters();
    }
    
    private void UpdateAnimatorParameters()
    {
        animator.SetBool("IsTravellingToRetreat", mIsTravellingToRetreat);
        animator.SetBool("IsRetreated", mIsRetreated);
        animator.SetBool("IsSwoopAttackAscending", mIsSwoopAttackAscending);
        animator.SetBool("IsSwoopAttackDescending", mIsSwoopAttackDescending);
        animator.SetBool("IsAlive", mIsAlive);
    }


    private void SetupSwoopAttack()
    {
        if (mSwoopAttackCooldownElapsed)
        {
            // Set initial location
            mSwoopAttackStartLocation = transform.position;

            // Choose an attack location
            mSwoopAttackLocation = player.position;

            // Choose an after-attack retreat location
            // TODO change this 
            mSwoopRetreatLocation = transform.position;

            mTimeSwoopAttackStarted = Time.time;
        }
    }

    private void SwoopAttack()
    {
        agent.enabled = false;
        
        // have the enemy swoop towards the player
        if (mIsSwoopAttackDescending)
        {
            float time = Mathf.Clamp01((Time.time - mTimeSwoopAttackStarted) / SwoopAttackDescentDuration);

            transform.position = Vector3.Lerp(mSwoopAttackStartLocation, mSwoopAttackLocation, time);
        }
        else if (mIsSwoopAttackAscending)
        {
            float time = Mathf.Clamp01((Time.time - mTimeSwoopAttackStarted - SwoopAttackDescentDuration) /
                                           SwoopAttackAscentDuration);

            transform.position = Vector3.Lerp(mSwoopAttackLocation, mSwoopRetreatLocation, time);
        }
    }

    private void RangedAttack()
    {
        if (mRangedAttackCooldownElapsed)
        {
            Projectile newProjectile = Instantiate(Projectile, GunPoint.transform.position, 
                Quaternion.LookRotation(player.transform.position - GunPoint.transform.position));
            newProjectile.Damage = RangedAttackDamage;
            newProjectile.Owner = gameObject;
            mTimeRangedAttackStarted = Time.time;
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

    private void SetupRetreat()
    {
        if (Random.value <= RetreatProbability && mWasHitLastFrame)
        {
            mRetreatStartPosition = transform.position;
            mRetreatEndPosition = RetreatPositions[Random.Range(0, RetreatPositions.Length - 1)];
            
            mTimeRetreatStarted = Time.time;    
        }

        mWasHitLastFrame = false;
    }

    private void Retreat()
    {
        agent.enabled = false;
        agent.updateRotation = false;
        
        if (mIsTravellingToRetreat)
        {
            float time = Mathf.Clamp01((Time.time - mTimeRetreatStarted) / RetreatTravelDuration);
            //transform.LookAt(player.transform.position - retreatPosition.transform.position);
            Vector3 position = Vector3.Lerp(mRetreatStartPosition, mRetreatEndPosition.transform.position, time);

            float x = position.x + Mathf.Sin(time * 2 * Mathf.PI * RetreatXFrequency) * RetreatXAmplitude;
            float y = position.y + Mathf.Sin(time * 2 * Mathf.PI * RetreatYFrequency) * RetreatYAmplitude;
            

            transform.LookAt(mRetreatEndPosition.transform.position);
            transform.position = new Vector3(x, y, position.z);;

        }
        // Otherwise they are retreated (mIsRetreated == true), since we know the enemy is retreating
        else
        {
            transform.LookAt(player.transform);
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
