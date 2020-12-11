using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class RangedEnemy : Enemy
{
    [Header("General")]
    [Tooltip("Game object enemy should be attacking")]
    public Transform _Target;
    public override Transform Target { get { return _Target; } set { _Target = value; } }
    [Tooltip("Health controller of enemy")]
    public HealthController _HealthController;
    public override HealthController HealthController => _HealthController;
    public override Conductor Conductor { get { return mConductor; } set { mConductor = value; } }

    private Conductor mConductor;
    
    private AudioSource mFireSFX;
    
    private NavMeshAgent agent;

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
    private int mAttackBeat;

    
    [Header("Retreat Variables")] 
    [Tooltip("The odds that an enemy will retreat after being hit (must be [0,1]).")]
    public float RetreatProbability;

    [Tooltip("How wide the triangle drawn behind the enemy used to determine where they will retreat to (must be [0,90]).")]
    public float RetreatAngleWidth;
    
    [Tooltip("Number of seconds the enemy will retreat for before taking another action.")]
    public float RetreatDuration;

    public float RetreatDistanceMin;
    public float RetreatDistanceMax;
    
    private float mRetreatTimeout;
    
    
    [Tooltip("Object to dissolve on death")]
    public Dissolve Dissolve;
    [Tooltip("Duration of dissolve effect")]
    public float DissolveTime = 2f;

    [Tooltip("Model which the Animator is attached to")]
    public Animator mAnimator;

    // States
    private bool mIsAlive;
    private bool mWasHitLastFrame;

    private float mTimeOfDeath = -10f;

    [FormerlySerializedAs("gunPoint")] public GameObject GunPoint;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _HealthController = GetComponentInChildren<HealthController>();
        mConductor = Conductor.GetActiveConductor();
        mFireSFX = GetComponent<AudioSource>();
        
        HealthController.OnDeath += OnDeath;
        HealthController.OnDamaged += OnDamaged;
        
        mChaseTimeout = -1;
        mRetreatTimeout = -RetreatDuration;

        mIsAlive = true;
        mWasHitLastFrame = false;

        mAttackBeat = Random.Range(0, mConductor.BarLength - 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (mIsAlive && Time.time - mRetreatTimeout > 5)
        {
            agent.updateRotation = true;

            bool playerInSightRange = Vector3.Distance(_Target.transform.position, transform.position) < SightRange;
            bool playerInAttackRange = Vector3.Distance(_Target.transform.position, transform.position) < AttackRange;

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
        
        // Dissolve out if dead.
        float dissolveProgress = (Time.time - mTimeOfDeath) / DissolveTime;
        if (dissolveProgress > 0 && dissolveProgress < 1)
        {
            Dissolve.SetDissolved(dissolveProgress);
        }

        WalkingAnimation();
    }

    private void AttackPlayer()
    {
        // Attack the player on a given interval
        // TODO Maybe make this interval random?
        if (mAttackBeat == mConductor.GetBeat() % mConductor.BarLength && Time.time >= mTimeLastAttack + TimeBetweenAttacks)
        {
            mTimeLastAttack = Time.time;
            mAnimator.SetTrigger("CastSpell");
            GetComponent<NavMeshAgent>().enabled = false;
            Invoke("FireBullet", 0.5f);
        }

        if (!CanChasePlayer)
        {
            transform.LookAt(new Vector3(_Target.transform.position.x, transform.position.y, _Target.transform.position.z));
        }
    }

    private void ChasePlayer()
    {
        // Change navigation destination every _rescanTime seconds
        if (Time.time - mChaseTimeout > ChaseDestinationComputeInterval)
        {
            mChaseTimeout = Time.time;
            
            Vector3 playerPosition = _Target.transform.position;

            float angle = Random.Range(0F, 360F);

            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * PersonalSpace;
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * PersonalSpace;

            Vector3 destination = new Vector3(playerPosition.x + x, playerPosition.y, playerPosition.z + z);
            agent.SetDestination(destination);
        }
        
        // If the agent is chasing, then disable automatic navmesh face direction and 
        agent.updateRotation = false;

        // Make the enemy look at the player when they are attacking
        Vector3 direction = _Target.transform.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, SnapToMultiplier * Time.deltaTime);
    }

    private void RetreatPlayer()
    {
        // Compute the retreat distance
        float retreatDistance = Random.Range(RetreatDistanceMin, RetreatDistanceMax);

        // Compute the retreat angle
        float retreatAngle = Random.Range(270 - RetreatAngleWidth, 270 + RetreatAngleWidth);

        // Compute the x,z coordinates for our retreat position
        float z = Mathf.Sin(Mathf.Deg2Rad * retreatAngle) * (transform.forward.z * retreatDistance);
        float x = Mathf.Cos(Mathf.Deg2Rad * retreatAngle) * (transform.forward.x * retreatDistance);

        // Compute destination position
        Vector3 destination = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);


        // Set destination
        agent.SetDestination(destination);
        
        // Set a timeout time so that no other actions are taken during retreat
        mRetreatTimeout = Time.time;
        
        // Reset last frame his indicator
        mWasHitLastFrame = false;
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
        mTimeOfDeath = Time.time;
        Destroy(gameObject, DissolveTime);
        if(agent != null)
            agent.isStopped = true;
        mIsAlive = false;
    }

    private void OnDamaged()
    {
        mWasHitLastFrame = true;
    }

    private void FireBullet()
    {
        Projectile newProjectile = Instantiate(Projectile, GunPoint.transform.position, Quaternion.LookRotation(_Target.transform.position - GunPoint.transform.position));
        newProjectile.Damage = AttackDamage;
        newProjectile.Owner = gameObject;
        mFireSFX.Play();
        if (CanChasePlayer || CanWander)
        {
            GetComponent<NavMeshAgent>().enabled = true;
        }
    }

    private void WalkingAnimation()
    {
        Vector3 normalizedMovement = GetComponent<NavMeshAgent>().desiredVelocity.normalized;
        Vector3 rightVector = Vector3.Project(normalizedMovement, transform.right);
        float rightVelocity = rightVector.magnitude * Vector3.Dot(rightVector, transform.right);
        if (rightVelocity > 0)
        {
            mAnimator.SetBool("WalkRight", true);
        }
        else
        {
            mAnimator.SetBool("WalkRight", false);
        }
        if (rightVelocity < 0)
        {
            mAnimator.SetBool("WalkLeft", true);
        }
        else
        {
            mAnimator.SetBool("WalkLeft", false);
        }
    }
}
