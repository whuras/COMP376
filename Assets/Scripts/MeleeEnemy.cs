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
    public override Conductor Conductor { get { return mConductor; } set { mConductor = value; } }
    [Tooltip("NavMesh agent of enemy")]
    public UnityEngine.AI.NavMeshAgent NavMeshAgent;
    [Tooltip("Animator of enemy")]
    public Animator Animator;
    [Tooltip("Object to dissolve on death")]
    public Dissolve Dissolve;
    [Tooltip("Duration of dissolve effect")]
    public float DissolveTime = 2f;
    [Tooltip("Origin of raycast used to check for successful attacks")]
    public Transform AttackOrigin;

    [Header("Mechanics")]
    [Tooltip("Max health of enemy")]
    public float Health = 20f;
    [Tooltip("Movement speed of enemy")]
    public float Speed = 5f;
    [Tooltip("Damage dealt by melee attacks")]
    public float Damage = 10f;
    [Tooltip("Range at which enemy can attack")]
    public float AttackRange = 1f;
    [Tooltip("Minimum time between enemy attacks")]
    public float AttackCooldown = 3f;

    float mTimeOfDeath = -10f;
    float mTimeLastAttack = -10f;
    
    private Conductor mConductor;

    void Start()
    {
        NavMeshAgent.speed = Speed;
        NavMeshAgent.stoppingDistance = 0f;
        NavMeshAgent.SetDestination(_Target.position);
        _HealthController.OnDeath += OnDeath;
        _HealthController.MaxHealth = Health;
        mConductor = Conductor.GetActiveConductor();
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
        TryStartAttack();
    }

    /// <summary> Start attack animation if within range of target. </summary>
    void TryStartAttack()
    {
        float distanceToTarget = (Target.position - transform.position).magnitude;
        if (distanceToTarget < AttackRange || Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            NavMeshAgent.enabled = false;
            if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                Animator.Play("Attack");
                mTimeLastAttack = Time.time;
            }
        }
        else
        {
            NavMeshAgent.enabled = true;
            NavMeshAgent.SetDestination(Target.position);
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
        Debug.Log("Attacks");
        Debug.DrawLine(AttackOrigin.position, AttackOrigin.position + AttackOrigin.forward.normalized * AttackRange, Color.red, 2f);
        for (int i = -1; i < 2; i++)
        {
            if (Physics.Raycast(AttackOrigin.position, AttackOrigin.forward + i * AttackOrigin.right, out RaycastHit hitInfo, AttackRange, 1 << 9))
            {
                HealthController other = hitInfo.transform.gameObject.GetComponent<HealthController>();
                if (other != null)
                {
                    other.TakeDamage(Damage);
                    return;
                }
            }
        }
    }
}
