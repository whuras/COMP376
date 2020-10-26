using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject Owner;
    public float Speed = 1f;
    public int Damage;
    public float TrajectoryCorrectionDistance = 2;
    public float CollisionRadius = 0.1f;
    public GameObject ImpactEffects;

    Vector3 mVelocity;
    Vector3 mTrajectoryCorrectionVector = new Vector3(0f, 0f, 0f);
    Vector3 mCompletedTrajectoryCorrection = new Vector3(0f, 0f, 0f);
    List<Collider> mIgnoredColliders;


    void Start()
    {
        mVelocity = transform.forward * Speed;
        mIgnoredColliders = new List<Collider>(Owner.GetComponentsInChildren<Collider>());

        if (TrajectoryCorrectionDistance >= 0)
        {
            mTrajectoryCorrectionVector = Vector3.ProjectOnPlane(Camera.main.transform.position - transform.position, Camera.main.transform.forward);
        }
    }

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

    void Hit(Collider collider)
    {
        HealthController target = collider.GetComponent<HealthController>();

        print(target);
        if (target != null)
        {
            target.TakeDamage(Damage);
        }

        if (ImpactEffects)
        {
            GameObject impactEffects = Instantiate(ImpactEffects, transform.position, Quaternion.LookRotation(-transform.forward));
            Destroy(impactEffects, 5f);
        }

        Destroy(this.gameObject);
    }
}
