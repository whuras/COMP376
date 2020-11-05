using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Magnitude of projectile velocity")]
    public float Speed = 1f;
    [Tooltip("Distance traveled by projectile before it reaches center of screen (negative values remove correction)")]
    public float TrajectoryCorrectionDistance = 2;
    [Tooltip("Size of collision sphere")]
    public float CollisionRadius = 0.1f;
    [Tooltip("Effects generated on projectile impact")]
    public GameObject ImpactEffects;

    Vector3 mVelocity;
    Vector3 mTrajectoryCorrectionVector = new Vector3(0f, 0f, 0f);
    Vector3 mCompletedTrajectoryCorrection = new Vector3(0f, 0f, 0f);
    
    /// <summary> Damage dealt by projectile. </summary>
    public float Damage
    {
        get => mDamage;
        set => mDamage = value;
    }
    float mDamage;
    
    /// <summary> Game object firing projectile. </summary>
    public GameObject Owner
    {
        get => mOwner;
        set => mOwner = value;
    }
    GameObject mOwner = null;
    List<Collider> mIgnoredColliders;


    /// <summary> Set up projectile trajectory. </summary>
    void Start()
    {
        // Prevent projectile from colliding with game object that fired it.
        mIgnoredColliders = new List<Collider>(Owner.GetComponentsInChildren<Collider>());

        // The projectile fired from a muzzle must hit a target at center of screen. To accomplish this, the trajectory must be corrected.
        mVelocity = transform.forward * Speed;
        if (TrajectoryCorrectionDistance >= 0)
        {
            mTrajectoryCorrectionVector = Vector3.ProjectOnPlane(Camera.main.transform.position - transform.position, Camera.main.transform.forward);
        }
    }

    /// <summary> Update projectile physics. </summary>
    void Update()
    {
        // Normal forward velocity
        Vector3 forwardDistanceThisFrame = mVelocity * Time.deltaTime;
        transform.position += forwardDistanceThisFrame;

        // Trajectory correction to center projectile on screen
        if (mCompletedTrajectoryCorrection.magnitude < mTrajectoryCorrectionVector.magnitude)
        {
            float portionOfCorrectionThisFrame = forwardDistanceThisFrame.magnitude / TrajectoryCorrectionDistance;
            Vector3 correctionThisFrame = mTrajectoryCorrectionVector * portionOfCorrectionThisFrame;
            correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, (mTrajectoryCorrectionVector-mCompletedTrajectoryCorrection).magnitude);
            mCompletedTrajectoryCorrection += correctionThisFrame;

            transform.position += correctionThisFrame;
        }

        // Hit detection
        Collider[] collisions = Physics.OverlapSphere(transform.position, CollisionRadius);
        if (collisions.Length > 0)
        {
            Hit(collisions[0]);
        }
    }

    /// <summary> Handle collisions with collidable objects. </summary>
    /// <param name="collider"> Object hit </param>
    void Hit(Collider collider)
    {
        // Deal damage if damageable object is hit.
        HealthController target = collider.GetComponent<HealthController>();
        if (target != null)
        {
            target.TakeDamage(Damage);
        }

        // Play impact effects if any exist.
        if (ImpactEffects)
        {
            GameObject impactEffects = Instantiate(ImpactEffects, transform.position, Quaternion.LookRotation(-transform.forward));
            Destroy(impactEffects, 5f);
        }

        Destroy(this.gameObject);
    }
}
