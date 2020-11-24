﻿using System.Collections;
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
    // <DECOUPLE FROM BREAKABLE>
    TutorialManager mTutorialManager;
    float mCrrtHealth;

    /// <summary> Cache components. </summary>
    private void Start()
    {
        mHealthController = gameObject.GetComponent<HealthController>();
        mHealthController.OnDeath += OnDamaged;
        mConductor = FindObjectOfType<Conductor>();
        mTutorialManager = FindObjectOfType<TutorialManager>();
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
        GameObject brokenBottle = Instantiate(DestroyedVersion, transform.position, transform.rotation, transform.parent);
        brokenBottle.transform.localScale = transform.localScale;

        // <DECOUPLE FROM BREAKABLE>
        if (!mTutorialManager.GetTutorialPhaseTwoStarted())
        {
            mTutorialManager.UpdateBottleCount(1);
        }
        // Remove non-destroyed version.
        Destroy(gameObject);
    }

    /// <summary> Remove health when bottle is damaged. We do this here instead of in the health controller because we want the damage to be binary. </summary>
    void OnDamaged()
    {
        // If beat was hit, deal demage.
        if (Mathf.Abs(mConductor.GetTimeToBeat()) < BeatThreshold)
        {
            // <DECOUPLE FROM BREAKABLE>
            // If on second phase of tutorial, add 1 to combo when shooting large bottle
            if (mTutorialManager.GetTutorialPhaseTwoStarted())
            {
                mTutorialManager.UpdateShotCount(1);
            }
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
            // <DECOUPLE FROM BREAKABLE>
            // If on second phase of tutorial, subtract 1 from combo when shooting large bottle
            if (mTutorialManager.GetTutorialPhaseTwoStarted())
            {
                mTutorialManager.UpdateShotCount(-1);
                if (mCrrtHealth < 5)
                {
                    mCrrtHealth++;
                }
            }
            if (MissedBeatSound)
            {
                mAudioSource.pitch = 1;
                mAudioSource.PlayOneShot(MissedBeatSound);
            }
        }
    }
}
