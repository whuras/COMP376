using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [Header("General")]
    [Tooltip("Game object enemy should be attacking")]
    public Transform _Target;
    public override Transform Target { get { return _Target; } set { _Target = value; } }
    [Tooltip("Health controller of enemy")]
    public HealthController _HealthController;
    public override HealthController HealthController => _HealthController;
    [Tooltip("NavMesh agent of enemy")]
    public UnityEngine.AI.NavMeshAgent NavMeshAgent;
    [Tooltip("Animator of enemy")]
    public Animator Animator;
    [Tooltip("Object to dissolve on death")]
    public Dissolve Dissolve;
    [Tooltip("Duration of dissolve effect")]
    public float DissolveTime = 2f;

    [Header("Mechanics")]
    [Tooltip("Max health of enemy")]
    public float Health = 20f;
    [Tooltip("Movement speed of enemy")]
    public float Speed = 5f;
    [Tooltip("Damage dealt by melee attacks")]
    public float Damage = 1f;
    [Tooltip("Range at which enemy can attack")]
    public float AttackRange = 1f;
    [Tooltip("Minimum time between enemy attacks")]
    public float AttackCooldown = 3f;

    float mTimeOfDeath = -10f;
    float mTimeLastAttack = -10f;
    

    void Start()
    {
        NavMeshAgent.speed = Speed;
        NavMeshAgent.stoppingDistance = AttackRange - 1f;
        _HealthController.OnDeath += OnDeath;
        _HealthController.MaxHealth = Health;
    }
    
    void Update()
    {
        // Dissolve out if dead.
        float dissolveProgress = (Time.time - mTimeOfDeath) / DissolveTime;
        if (dissolveProgress > 0 && dissolveProgress < 1)
        {
            Dissolve.SetDissolved(dissolveProgress);
            return;
        }

        // Attack if within range
        NavMeshAgent.SetDestination(_Target.position);
        TryStartAttack();
    }

    /// <summary> Start attack animation if within range of target. </summary>
    void TryStartAttack()
    {
        if (NavMeshAgent.remainingDistance < AttackRange)
        {
            if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                Animator.Play("Attack");
                mTimeLastAttack = Time.time;
            }
        }
        else
        {
            if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Running") && NavMeshAgent.velocity.magnitude != 0)
            {
                Animator.Play("Running");
            }
        }
    }

    /// <summary> Record time of death for dissolve effect and queue game object destruction. </summary>
    void OnDeath()
    {
        mTimeOfDeath = Time.time;
        Destroy(HealthController.gameObject);
        Destroy(gameObject, DissolveTime);
    }

    /// <summary> Should be called by trigger within attack animation. Checks if player is within melee range and deals appropriate amount of damage. </summary>
    public void Attack()
    {
        // REQUIRES IMPLEMENTATION
    }
}
