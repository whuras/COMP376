using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Example enemy used for testing. </summary>
public class Target : MonoBehaviour
{
    [Tooltip("Heads up display")]
    public PlayerInterface PlayerHUD;

    HealthController mHealthController;
    Animator mAnimator;

    /// <summary> Get references and add actions. </summary>
    void Start()
    {
        mHealthController = gameObject.GetComponent<HealthController>();
        mAnimator = gameObject.GetComponent<Animator>();
        mHealthController.OnDamaged += OnDamaged;
        mHealthController.OnDeath += OnDeath;
    }

    /// <summary> Action called when target is damaged. </summary>
    void OnDamaged()
    {
        mAnimator.Play("Damaged", -1, 0f);
        if (PlayerHUD)
        PlayerHUD.SetHealthDisplayed(mHealthController.NormalizedHealth);
    }
    
    /// <summary> Action called when target dies. </summary>
    void OnDeath()
    {
        mAnimator.Play("Dead");
    }
}
