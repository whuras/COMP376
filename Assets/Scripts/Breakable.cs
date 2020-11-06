﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable: MonoBehaviour
{
    public GameObject DestroyedVersion;
    public AudioClip MissedBeatSound;
    public AudioClip HitBeatSound;
    public float BeatThreshold = 0.15f;

    HealthController mHealthController;
    Conductor mConductor;
    AudioSource mAudioSource;
    Animator mAnimator;

    private void Start()
    {
        mConductor = GameObject.Find("Conductor").GetComponent<Conductor>();
        mHealthController = gameObject.GetComponent<HealthController>();
        mHealthController.OnDeath += OnDeath;
        mHealthController.OnDamaged += OnDamaged;
        if (MissedBeatSound)
        {
            mAudioSource = gameObject.GetComponent<AudioSource>();
        }
        mAnimator = gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        mAnimator.SetFloat("TimeToBeat",Mathf.Abs(mConductor.GetTimeToBeat()));
    }
    
    void Destroy()
    {
        Instantiate(DestroyedVersion, transform.position, transform.rotation, transform.parent);
        Destroy(gameObject);

        if (tag == "Whiskey")
        {
            FindObjectOfType<TutorialManager>().UpdateBottleCount(1);
        }
    }

    void OnDamaged()
    {
        if (Mathf.Abs(mConductor.GetTimeToBeat()) < BeatThreshold)
        {
            if (HitBeatSound)
            {
                mAudioSource.pitch = 1.5f - mHealthController.NormalizedHealth;
                mAudioSource.clip = HitBeatSound;
                mAudioSource.Play();
            }
        }
        else
        {
            if (MissedBeatSound)
            {
                mAudioSource.pitch = 1;
                mAudioSource.PlayOneShot(MissedBeatSound);
            }
        }
    }

    void OnDeath()
    {
        if (Mathf.Abs(mConductor.GetTimeToBeat()) < BeatThreshold)
        {
            Destroy();
        }
        else
        {
            if (MissedBeatSound)
            {
                mAudioSource.PlayOneShot(MissedBeatSound);
            }
        }
    }
}
