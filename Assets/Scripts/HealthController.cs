﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summar> Handles health of damageable object. </summary>
public class HealthController : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Max health of damageable object")]
    public float MaxHealth;
    [Tooltip("Timeframe after damage is taken during which object is invulnerable.")]
    public float InvulnerabilityTime = 0f;

    [SerializeField]
    public float CurrentHealth => mCrrtHealth;

    [Header("Actions")]
    [Tooltip("What to do when object is damaged")]
    public UnityAction OnDamaged;
    [Tooltip("What to do when object is healed")]
    public UnityAction OnHealed;
    [Tooltip("What to do when object dies")]
    public UnityAction OnDeath;

    [Tooltip("Toggles being able to take damage")]
    public bool canTakeDamage = true;
    [Tooltip("Toggles being able to heal")]
    public bool canHeal = true;

    float mCrrtHealth;
    float mLastTimeDamageTaken = -10f;

    /// <summary> Health of object normalized from 0 to 1. </summary>
    public float NormalizedHealth
    {
        get => mCrrtHealth / MaxHealth;
    }

    /// <summary> Setup health controller. </summary>
    void Start()
    {
        mCrrtHealth = MaxHealth;
    }

    /// <summary> Deduct health from object </summary>
    /// <param name="damage"> Amount of damage dealt </param>
    public void TakeDamage(float damage)
    {
        if (canTakeDamage){
            // Return early if object is currently invulnerable
            if (mLastTimeDamageTaken + InvulnerabilityTime > Time.time)
            {
                return;
            }
            mLastTimeDamageTaken = Time.time;
            if (mCrrtHealth < 0)
            {
                mCrrtHealth = 0;
                OnDeath?.Invoke();
            }
            else
            {
                mCrrtHealth -= damage;
                if (mCrrtHealth < 0)
                {
                    mCrrtHealth = 0;
                    OnDeath?.Invoke();
                }
                else
                {
                    OnDamaged?.Invoke();
                }
            }
        }
    }

    /// <summary> Add health to object </summary>
    /// <param name="damage"> Amount of health added </param>
    public void Heal(float health)
    {
        if (canHeal)
        {
            mCrrtHealth += health;
            if (mCrrtHealth > MaxHealth)
            {
                mCrrtHealth = MaxHealth;
            }

            OnHealed?.Invoke();
        }
    }
}