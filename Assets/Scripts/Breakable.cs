using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Destroyed version of object (after destruction)")]
    public GameObject DestroyedVersion;
    [Tooltip("Sound played when shot off beat")]
    public AudioClip MissedBeatSound;
    [Tooltip("Sound played when shot on beat but not destroyed")]
    public AudioClip HitBeatSound;
    [Tooltip("Maximum time to beat that object can be destroyed during")]
    public float BeatThreshold = 0.15f;
    [Tooltip("Number of hits required to destroy")]
    public int Health = 1;

    HealthController mHealthController;
    Conductor mConductor;
    AudioSource mAudioSource;
    float mCrrtHealth;

    /// <summary> Cache components. </summary>
    private void Start()
    {
        mHealthController = gameObject.GetComponent<HealthController>();
        mHealthController.OnDeath += OnDamaged;
        mConductor = Conductor.GetActiveConductor();
        if (MissedBeatSound)
        {
            mAudioSource = gameObject.GetComponent<AudioSource>();
        }
        mCrrtHealth = Health;
    }

    /// <summary> Destroy object. </summary>
    void Destroy()
    {
        // Instantiate destroyed version of object.
        GameObject brokenObject = Instantiate(DestroyedVersion, transform.position, transform.rotation, transform.parent);
        brokenObject.transform.localScale = transform.localScale;

        // Remove non-destroyed version.
        Destroy(gameObject);
    }

    /// <summary> Remove health when bottle is damaged. We do this here instead of in the health controller because we want the damage to be binary. </summary>
    void OnDamaged()
    {
        // If beat was hit, deal demage.
        if (Mathf.Abs(mConductor.GetTimeToBeat()) < BeatThreshold)
        {
            // Play positive hit sound if bottle was not destroyed and one exists.
            if (--mCrrtHealth > 0)
            {
                if (HitBeatSound)
                {
                    mAudioSource.pitch = 1.5f - (mCrrtHealth / Health);
                    mAudioSource.clip = HitBeatSound;
                    mAudioSource.Play();
                }
            }
            // If bottle was destroyed, destroy it.
            else
            {
                Destroy();
            }
        }
        // Else, play missed beat sfx.
        else
        {
            if (MissedBeatSound)
            {
                mAudioSource.pitch = 1;
                mAudioSource.PlayOneShot(MissedBeatSound);
            }
        }
    }
}
