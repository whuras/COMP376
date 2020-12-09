using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Example enemy used for testing. </summary>
public class Target : MonoBehaviour
{
    HealthController mHealthController;
    Animator mAnimator;

    /// <summary> Get references and add actions. </summary>
    void Start()
    {
        mHealthController = gameObject.GetComponent<HealthController>();
        if (mHealthController == null)
        {
            mHealthController = gameObject.GetComponentInChildren<HealthController>();
        }
        mAnimator = gameObject.GetComponent<Animator>();
        
        mHealthController.OnDamaged += OnDamaged;
        mHealthController.OnDeath += OnDeath;
    }

    /// <summary> Action called when target is damaged. </summary>
    void OnDamaged()
    {
        mAnimator.Play("Damaged", -1, 0f);
    }
    
    /// <summary> Action called when target dies. </summary>
    void OnDeath()
    {
        mAnimator.Play("Dead");
    }
}
