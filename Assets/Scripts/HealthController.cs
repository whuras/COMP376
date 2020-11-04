using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    public UnityAction OnDamaged;
    public UnityAction OnHealed;
    public UnityAction OnDeath;

    public float healthAmount;
    public float healthAmountMaximum;

    public void TakeDamage(float damage)
    {
        healthAmount -= damage;
        if (healthAmount < 0)
        {
            healthAmount = 0;
            OnDeath();
        }
        else
        {
            OnDamaged();
        }
    }

    public void Heal(float health)
    {
        if (healthAmount + health > healthAmountMaximum)
        {
            healthAmount = healthAmountMaximum;
        }
        
        OnHealed();
    }

    public float GetHealthNormalized()
    {
        return (float)healthAmount / healthAmountMaximum;
    }
}