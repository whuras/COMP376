using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gargoyle : Enemy
{
    [Tooltip("Game object gargoyle should be attacking")]
    public Transform _Target;
    public override Transform Target { get { return _Target; } set { _Target = value; } }
    [Tooltip("Health controller of gargoyle")]
    public HealthController _HealthController;
    public override HealthController HealthController => _HealthController;
    public override Conductor Conductor { get { return mConductor; } set { mConductor = value; } }
    [Tooltip("Source of projectiles")]
    public Transform MuzzlePosition;
    [Tooltip("Rendered object to dissolve when enemy is killed")]
    public Dissolve Dissolve;
    [Tooltip("Time taken to dissolve enemy on death")]
    public float DissolveTime = 3f;
    [Tooltip("Projectile to fire")]
    public Projectile Projectile;
    [Tooltip("Fire sound effects")]
    public AudioClip FireSFX;
    [Tooltip("Swoop sound effects")]
    public AudioClip SwoopSFX;
    [Tooltip("Audio Source to play sound effects from")]
    public AudioSource AudioSource;
    [Tooltip("Movement speed of enemy")]
    public float MovementSpeed = 3f;
    [Tooltip("Damage dealt by swoop")]
    public float SwoopDamage;

    private Conductor mConductor;
    
    public Vector3 Destination
    {
        get => mDestination;
        set
        {
            //transform.position = value;
            mDestination = value;
            mHasDestination = value != Vector3.zero;
        }
    }
    Vector3 mDestination = Vector3.zero;
    bool mHasDestination = false;

    public int FireBeat = 0;
    public int FireRate = 8;

    bool CanFire = true;
    bool HasNotFired = true;
    float mTimeOfDeath = -10f;
    float SwoopStart = -10f;
    float SwoopTime = 2f;
    float SwoopApex = -10f;
    float SwoopDistance = 35f;
    Vector3 SwoopDirection;
    Vector3 SwoopStartPos;
    float SwoopHeight;
    bool mSwoopHasHit = false;

    Quaternion mRotation;

    // Start is called before the first frame update
    void Start()
    {
        mConductor = Conductor.GetActiveConductor();
        HealthController.OnDeath += OnDeath;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Swoop();
        
        // Dissolve out if dead
        float dissolveProgress = (Time.time - mTimeOfDeath) / DissolveTime;
        if (dissolveProgress < 1 && dissolveProgress > 0)
        {
            Dissolve.SetDissolved(dissolveProgress);
            return;
        }

        Fire();
        transform.rotation = Quaternion.Lerp(transform.rotation, mRotation, Time.deltaTime * 5f);
    }

    /// <summary> Move if appropriate. </summary>
    void Move()
    {
        // Return early if no destination.
        if (mHasDestination == false)
        {
            return;
        }

        // Return early if destination is already reached.
        Vector3 toDestination = Destination - transform.position;
        if (toDestination.magnitude < 0.1f)
        {
            return;
        }

        // Head towards destination otherwise.
        transform.position += toDestination.normalized * MovementSpeed * Time.deltaTime;
        //SwoopStartPos += toDestination.normalized * MovementSpeed * Time.deltaTime;
    }

    /// <summary> Fire projectile if appropriate. </summary>
    void Fire()
    {
        if (!CanFire)
        {
            return;
        }
        // Fire on appropriate beats
        if (Conductor.GetBeat() % FireRate != FireBeat)
        {
            HasNotFired = true;
            return;
        }
        else if (HasNotFired)
        {
            Projectile newProjectile = Instantiate(Projectile, MuzzlePosition.position, Quaternion.LookRotation(MuzzlePosition.forward));
            newProjectile.Owner = HealthController.gameObject;
            newProjectile.Damage = 5f;
            AudioSource.PlayOneShot(FireSFX, 0.3f);
            HasNotFired = false;
        }
    }

    /// <summary> Swoop down. </summary>
    void Swoop()
    {
        float x = (Time.time - SwoopStart) / SwoopTime;
        if (x > 1)
        {
            CanFire = true;

            // Turn to face target
            Vector3 toTarget = Target.position - transform.position;
            mRotation = Quaternion.LookRotation(toTarget);
            return;
        }

        float denom = (x > SwoopApex) ? 1-SwoopApex : SwoopApex;
        float y = Mathf.Pow((x - SwoopApex) / denom, 2);

        Vector3 pos = SwoopDirection * x;
        pos.y = -SwoopHeight * (1-y);

        Vector3 newPosition = SwoopStartPos + pos;
        Vector3 up = newPosition - transform.position;
        Vector3 left = Vector3.Cross(up, Vector3.up);
        Vector3 forward = Vector3.Cross(up, left);
        transform.position = newPosition;
        mRotation = Quaternion.LookRotation(forward, up);
        Debug.DrawLine(transform.position, transform.position+up.normalized * 5f, Color.green);
        Debug.DrawLine(transform.position, transform.position+left.normalized * 5f, Color.red);
        Debug.DrawLine(transform.position, transform.position+forward.normalized * 5f, Color.blue);

        // Check for collisions
        // Return early if swoop has already hit player
        if (mSwoopHasHit)
        {
            return;
        }
        // Continue check otherwise
        foreach (Collider col in Physics.OverlapSphere(transform.position, 2.5f, 1 << 9))
        {
            HealthController other = col.gameObject.GetComponent<HealthController>();
            if (other != null)
            {
                Debug.Log("HitPlayer");
                other.TakeDamage(SwoopDamage);
                mSwoopHasHit = true;
            }
        }
    }

    public void StartSwoop(Vector3 target)
    {
        if (Time.time - SwoopStart < SwoopTime)
        {
            return;
        }
        AudioSource.PlayOneShot(SwoopSFX);
        CanFire = false;
        HasNotFired = false;
        mSwoopHasHit = false;
        SwoopStart = Time.time;
        SwoopDirection = target - transform.position;
        SwoopHeight = -1.5f-SwoopDirection.y;
        SwoopDirection.y = 0;
        SwoopApex = SwoopDirection.magnitude / SwoopDistance;
        SwoopDirection = SwoopDirection.normalized * SwoopDistance;
        SwoopStartPos = transform.position;
    }

    void OnDeath()
    {
        mTimeOfDeath = Time.time;
        Destroy(HealthController.gameObject);
        Destroy(gameObject, DissolveTime);
    }
}
